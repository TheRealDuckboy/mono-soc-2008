//
// Unit test for Gendarme.Reporter.GenerateDefectsPerAssemblyAction
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
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace Gendarme.Reporter.Test {
	[TestFixture]
	public class GenerateDefectsPerAssemblyActionTest {
		static readonly string xmlFile = "Test/Fakes/06-18-2008.xml";
		static readonly string sampleGeneratedFile = "Mono.Security.xml";

		private void GetProcessedDocument ()
		{
			XDocument document = XDocument.Load (xmlFile);
			Assert.IsNotNull (document);

			document = new GenerateDefectsPerAssemblyAction ().Process (document)[0];
			Assert.IsNotNull (document);
		}

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			GetProcessedDocument ();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			foreach (string file in Directory.GetFiles (Directory.GetCurrentDirectory (), "*.xml")) 
				File.Delete (file);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PassMoreThanOneDocumentTest ()
		{
			XDocument document = XDocument.Load (xmlFile);
			new GenerateDefectsPerAssemblyAction ().Process (document, document);
		}

		[Test]
		public void WrittenFilesTest ()
		{
			Assert.IsTrue (File.Exists (sampleGeneratedFile));
			Assert.AreEqual (72, Directory.GetFiles (Directory.GetCurrentDirectory (), "*.xml").Length);
		}

		[Test]
		public void FilesSectionTest ()
		{
			//Only one file per xml file
			XDocument document = XDocument.Load (sampleGeneratedFile);
			Assert.AreEqual (1, document.Root.Element ("files").Elements ().Count ());		
		}

		[Test]
		public void RulesSectionTest ()
		{
			//All rules in all xml files.
			XDocument document = XDocument.Load (sampleGeneratedFile);
			Assert.AreEqual (19, document.Root.Element ("rules").Elements ().Count ());
		}

		[Test]
		public void ResultSectionTest ()
		{
			XDocument document = XDocument.Load (sampleGeneratedFile);
			Assert.AreEqual (18, CountDefects (document));
		}

		private int CountDefects (XDocument document)
		{
			int counter = 0;
			
			foreach (XElement rule in document.Root.Element ("results").Elements ()) 
				foreach (XElement target in rule.Elements ("target")) 
					foreach (XElement defect in target.Elements ("defect")) 
						counter++;
			
			return counter;
		}

		[Test]
		public void ResultSectionWithoutTargetsTest ()
		{
			XDocument document = XDocument.Load (sampleGeneratedFile);
			foreach (XElement rule in document.Root.Element ("results").Elements ()) {
				Assert.IsTrue (rule.Elements ("target").Count () > 0, "Shouldn't be rules without targets");
			}
		}
	}
}
