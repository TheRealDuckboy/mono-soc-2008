//
// Gendarme.Rules.Serialization.CallBaseMethodsOnISerializableTypesRule
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
using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Mono.Cecil;

namespace Gendarme.Rules.Serialization {
	public class CallBaseMethodsOnISerializableTypesRule : Rule, ITypeRule {

		private static bool InheritsFromISerializableImplementation (TypeReference type)
		{
			TypeDefinition current = type.Resolve ();
			if (current == null || current.FullName == "System.Object")
				return false;
			if (current.IsSerializable && current.Implements ("System.Runtime.Serialization.ISerializable"))
				return true;

			return InheritsFromISerializableImplementation (current.BaseType);
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!InheritsFromISerializableImplementation (type.BaseType))
				return RuleResult.DoesNotApply;
			return Runner.CurrentRuleResult;
		}
	}
}
