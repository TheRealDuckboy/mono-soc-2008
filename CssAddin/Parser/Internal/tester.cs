//
// tester.cs -- Used to test the parser operations
//
// Authors: Andreas Louca (andreas@louca.org)
//
// Copyright (C) 2008 Andreas Louca
//

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace CssAddin.Parser.Internal 
{
	class tester 
	{
		static void Main(string[] args)
		{
			TextReader cssfileReader = null;
			string filename = null;
			
			
			if (args.Length < 1) {
				Console.WriteLine("CssEditor Parser Tester\n\nSyntax: tester.exe <cssfile.css>");
				return;
			}
			else {
				filename = args[0];
				Console.WriteLine("CssEditor Parser tester: parsing {0}", args[0]);
			}
			
		//	try {
				cssfileReader = new StreamReader(filename);
							
				//CssTokenizer testTokenizer = new CssTokenizer ();
				CssRecursiveParser testParser = new CssRecursiveParser (cssfileReader);
			testParser.Parse ();
			
				//testTokenizer.ParseDocument (cssfileReader);				
		//	} 
		//	catch (Exception e)
		//	{
		//		Console.WriteLine("Boing: ", e.ToString ());
		//		return;
		//	}
		}
	}
}