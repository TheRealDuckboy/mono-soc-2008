//
// Unit tests for AttributeStringLiteralShouldParseCorrectlyRule
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
using Gendarme.Rules.Correctness;
using Mono.Cecil;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;
using Test.Rules.Helpers;
using NUnit.Framework;

namespace Test.Rules.Correctness {
	[AttributeUsage (AttributeTargets.All)]
	public class ValidSince : Attribute {
		public ValidSince (string assemblyVersion)
		{
		}
	}

	[AttributeUsage (AttributeTargets.All)]
	public class Reference : Attribute {
		public Reference (string url)
		{
		}
	}

	[AttributeUsage (AttributeTargets.All)]
	public class Uses : Attribute {
		public Uses (string guid)
		{
		}
	}

	[TestFixture]
	public class AttributeStringLiteralsShouldParseCorrectlyMethodTest : MethodRuleTestFixture<AttributeStringLiteralsShouldParseCorrectlyRule> {
		[Test]
		public void SkipOnAttributelessMethodsTest ()
		{
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}
		
		[ValidSince ("1.0.0.0")]
		[Reference ("http://www.mono-project.com/Gendarme")]
		[Uses ("00000101-0000-0000-c000-000000000046")]
		public void WellAttributedMethod ()
		{
		}

		[Test]
		public void SuccessOnWellAttributedMethodTest ()
		{
			AssertRuleSuccess<AttributeStringLiteralsShouldParseCorrectlyMethodTest> ("WellAttributedMethod");
		}

		[ValidSince ("foo")]
		[Reference ("bar")]
		[Uses ("0")]
		public void BadAttributedMethod ()
		{
		}

		[Test]
		public void FailOnBadAttributedMethodTest ()
		{
			AssertRuleFailure<AttributeStringLiteralsShouldParseCorrectlyMethodTest> ("BadAttributedMethod", 3);
		}
	}

	[TestFixture]
	public class AttributeStringLiteralsShouldParseCorrectlyTypeTest : TypeRuleTestFixture<AttributeStringLiteralsShouldParseCorrectlyRule> {
		
		[Test]
		public void SkipOnAttributelessTypesTest ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
		}
		
		[ValidSince ("1.0.0.0")]
		[Reference ("http://www.mono-project.com/Gendarme")]
		[Uses ("00000101-0000-0000-c000-000000000046")]
		class WellAttributedClass {
		}

		[Test]
		public void SuccessOnWellAttributedClassTest ()
		{
			AssertRuleSuccess<WellAttributedClass> ();
		}


		[ValidSince ("foo")]
		[Reference ("bar")]
		[Uses ("0")]	
		class BadAttributedClass {
		}

		[Test]
		public void FailOnBadAttributedClassTest ()
		{
			AssertRuleFailure<BadAttributedClass> (3);
		}

		class WellAttributedClassWithFields {
			[ValidSince ("1.0.0.0")]
			[Reference ("http://www.mono-project.com/Gendarme")]
			[Uses ("00000101-0000-0000-c000-000000000046")]
			object obj;

		}

		[Test]
		public void SuccessOnWellAttributedClassWithFieldsTest () {
			AssertRuleSuccess<WellAttributedClassWithFields> ();
		}

		class BadAttributedClassWithFields {
			[ValidSince ("foo")]
			[Reference ("bar")]
			[Uses ("0")]	
			int foo;
		}

		[Test]
		public void FailOnBadAttributedClassWithFieldsTest ()
		{
			AssertRuleFailure<BadAttributedClassWithFields> (3);
		}
	}

	[TestFixture]
	public class AttributeStringLiteralsShouldParseCorrectlyAssemblyTest : AssemblyRuleTestFixture<AttributeStringLiteralsShouldParseCorrectlyRule>{
		private AssemblyDefinition GenerateFakeAssembly (string version, string url, string guid)
		{
			AssemblyDefinition definition = DefinitionLoader.GetAssemblyDefinition (this.GetType ());
			CustomAttribute attribute = new CustomAttribute (DefinitionLoader.GetMethodDefinition<ValidSince> (".ctor", new Type[] {typeof (string)}));
			attribute.ConstructorParameters.Add (version);
			definition.CustomAttributes.Add (attribute);

			attribute = new CustomAttribute (DefinitionLoader.GetMethodDefinition<Reference> (".ctor", new Type[] {typeof (string)}));
			attribute.ConstructorParameters.Add (url);
			definition.CustomAttributes.Add (attribute);

			attribute = new CustomAttribute (DefinitionLoader.GetMethodDefinition<Uses> (".ctor", new Type[] {typeof (string)}));
			attribute.ConstructorParameters.Add (guid);
			definition.CustomAttributes.Add (attribute);

			return definition;
		}

		[Test]
		public void SuccessOnWellAttributedAssemblyTest ()
		{
			AssertRuleSuccess (GenerateFakeAssembly ("1.0.0.0", "http://www.mono-project.com/Gendarme", "00000101-0000-0000-c000-000000000046"));
		}

		[Test]
		public void FailOnBadAttributedAssemblyTest ()
		{
			AssertRuleFailure (GenerateFakeAssembly ("foo", "bar", "0"), 3);
		}
	}
}
