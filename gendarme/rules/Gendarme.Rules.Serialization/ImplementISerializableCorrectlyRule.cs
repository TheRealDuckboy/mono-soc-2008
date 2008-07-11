//
// Gendarme.Rules.Serialization.ImplementISerializableCorrectlyRule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2008 Néstor Salceda
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Helpers;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Serialization {
	[Problem ("Although you are implementing the ISerializable interface, there are some fields that aren't going to be serialized and aren't marked with the [NonSerialized] attribute.")]
	[Solution ("Mark with the [NonSerialized] attribute the field. This helps developers to understand better your code, and perhaps to discover quickly some errors.")]
	public class ImplementISerializableCorrectlyRule : Rule, ITypeRule {
		static MethodSignature addValueSignature = new MethodSignature ("AddValue", "System.Void");
	
		private static bool IsCallingToSerializationInfoAddValue (Instruction instruction)
		{
			if (instruction == null || instruction.OpCode.FlowControl != FlowControl.Call)
				return false; 
			MethodReference method = (MethodReference) instruction.Operand;
			return addValueSignature.Matches (method) && String.Compare (method.DeclaringType.FullName, "System.Runtime.Serialization.SerializationInfo") == 0;
		}

		private static IList<FieldReference> GetFieldsUsedIn (MethodDefinition method)
		{
			IList<FieldReference> result = new List<FieldReference> ();

			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode == OpCodes.Ldfld && IsCallingToSerializationInfoAddValue (instruction.Next))
					result.Add ((FieldReference) instruction.Operand);
			}
			return result;
		}

		private void CheckUnusedFieldsIn (TypeDefinition type, MethodDefinition getObjectData)
		{
			IList<FieldReference> fieldsUsed = GetFieldsUsedIn (getObjectData);
			
			foreach (FieldDefinition field in type.Fields) {
				if (!fieldsUsed.Contains (field) && !field.IsNotSerialized && !field.IsStatic)
					Runner.Report (type, Severity.High, Confidence.Normal, String.Format ("The field {0} isn't going to be serialized, please use the [NonSerialized] attribute.", field.Name));
			}
		}

		private void CheckExtensibilityFor (TypeDefinition type, MethodDefinition getObjectData)
		{
			if (!type.IsSealed && getObjectData.IsFinal)
				Runner.Report (type, Severity.High, Confidence.Normal, String.Format ("If this class is going to be sealed, seal it; else you should make virtual the GetObjectData method."));
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.IsSerializable || !type.Implements ("System.Runtime.Serialization.ISerializable"))
				return RuleResult.DoesNotApply;
			
			MethodDefinition getObjectData = type.GetMethod (MethodSignatures.GetObjectData);
			if (getObjectData != null) {
				CheckUnusedFieldsIn (type, getObjectData);
				CheckExtensibilityFor (type, getObjectData);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
