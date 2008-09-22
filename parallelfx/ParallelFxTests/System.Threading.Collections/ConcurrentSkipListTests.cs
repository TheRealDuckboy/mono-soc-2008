// ConcurrentSkipListTests.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.Threading;
using System.Threading.Collections;

using NUnit;
using NUnit.Framework;

namespace ParallelFxTests
{
	[TestFixtureAttribute]
	public class ConcurrentSkipListTests
	{
		ConcurrentSkipList<int> skiplist;

		[SetUpAttribute]
		public void Setup()
		{
			skiplist = new ConcurrentSkipList<int>();
		}

		void AddStuff()
		{
			skiplist.Add(1);
			skiplist.Add(2);
			skiplist.Add(3);
			skiplist.Add(4);
		}

		[TestAttribute]
		public void AddTestCase()
		{
			Assert.IsTrue(skiplist.Add(1), "#1");
		}

		[TestAttribute]
		public void EnumerateTestCase()
		{
			AddStuff();
			
			string s = string.Empty;
			foreach (int i in skiplist)
				s += i.ToString();

			Assert.AreEqual("1234", s);
		}

		[TestAttribute]
		public void ToArrayTestCase()
		{
			int[] expected = new int[] { 1, 2, 3, 4 };
			AddStuff();
			int[] array = skiplist.ToArray();
			CollectionAssert.AreEqual(expected, array, "#1");

			Array.Clear(array, 0, array.Length);
			skiplist.CopyTo(array, 0);
			CollectionAssert.AreEqual(expected, array, "#2");
		}
		 
	}
}
