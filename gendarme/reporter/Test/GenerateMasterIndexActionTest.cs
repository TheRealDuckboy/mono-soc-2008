//
// Unit test for Gendarme.Reporter.GenerateMasterIndexAction
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
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Gendarme.Reporter;

namespace Gendarme.Reporter.Test {
	[TestFixture]
	public class GenerateMasterIndexActionTest {
		static readonly string xmlFile = "Test/Fakes/06-18-2008.xml";
		XDocument document;

		[TestFixtureSetUp]
		private void FixtureSetUp ()
		{
			document = GetProcessedDocument ();
		}

		private XDocument GetProcessedDocument () 
		{
			XDocument document = XDocument.Load (xmlFile);
			Assert.IsNotNull (document);

			document = new GenerateMasterIndexAction ().Process (document)[0];
			Assert.IsNotNull (document);
			return document;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PassMoreThanOneDocumentTest ()
		{
			new GenerateMasterIndexAction ().Process (document, document);
		}

		[Test]
		public void HeaderSectionTest ()
		{
			XElement element = document.Element ("gendarme-output");
			Assert.IsNotNull (element);
			
			Assert.AreEqual (XDocument.Load (xmlFile).Element ("gendarme-output").Attribute ("date").Value, element.Attribute ("date").Value);
		}

		[Test]
		public void GeneratedAssembliesSectionTest ()
		{
			XElement element = document.Root.Element ("assemblies");
			Assert.IsNotNull (element);
			
			Assert.AreEqual (72, element.Elements ().Count ());

			Assert.AreEqual ("Accessibility", element.Elements ().First ().Attribute ("shortname").Value);
			Assert.AreEqual ("System.Xml.Linq", element.Elements ().Last ().Attribute ("shortname").Value);
		}

		[Test]
		public void CheckGeneratedAssemblyTest ()
		{
			XElement first = document.Root.Element ("assemblies").Elements ().First ();
			
			Assert.IsNotNull (first.Attribute ("critical"));
			Assert.IsNotNull (first.Attribute ("high"));
			Assert.IsNotNull (first.Attribute ("medium"));
			Assert.IsNotNull (first.Attribute ("low"));
		}

		[Test]
		public void CheckCountedDefectsTest ()
		{
			var query = (from assembly in document.Root.Element ("assemblies").Elements ()
				where assembly.Attribute ("shortname").Value.Equals ("mscorlib")
				select assembly).ToList ();
			
			
			Assert.IsNotNull (query);
			Assert.AreEqual (1, query.Count ());
			Assert.IsNotNull (query.First ());

			Assert.AreEqual (0, Int32.Parse (query.First ().Attribute ("critical").Value));
			Assert.AreEqual (234, Int32.Parse (query.First ().Attribute ("high").Value));
			Assert.AreEqual (131, Int32.Parse (query.First ().Attribute ("medium").Value));
			Assert.AreEqual (2, Int32.Parse (query.First ().Attribute ("low").Value));
		}
	}
}
