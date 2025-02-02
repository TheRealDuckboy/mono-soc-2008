//#if NET_4_0

using System;
using System.Threading;
using System.Collections.Concurrent;

using NUnit;
using NUnit.Framework;

namespace ParallelFxTests
{
	[TestFixture]
	public class ConcurrentDictionaryTests
	{
		ConcurrentDictionary<string, int> map;
		
		[SetUp]
		public void Setup()
		{
			map = new ConcurrentDictionary<string, int>();
			AddStuff();
		}
		
		void AddStuff()
		{
			map.Add("foo", 1);
			map.Add("bar", 2);
			map.Add("foobar", 3);
		}
		
		[Test]
		public void AddWithoutDuplicateTest()
		{
			map.Add("baz", 2);
			
			Assert.AreEqual(2, map.GetValue("baz"));
			Assert.AreEqual(2, map["baz"]);
			Assert.AreEqual(4, map.Count);
		}
		
		[Test]
		public void AddParallelWithoutDuplicateTest ()
		{
			ParallelTestHelper.Repeat (delegate {
				Setup ();
				int index = 0;
				
				ParallelTestHelper.ParallelStressTest (map, delegate {
					int own = Interlocked.Increment (ref index);
					
					while (!map.TryAdd ("monkey" + own.ToString (), 3));
				}, 4);
				
				Assert.AreEqual (7, map.Count);
				int value;
				
				Assert.IsTrue (map.TryGetValue ("monkey1", out value), "#1");
				Assert.AreEqual (3, value, "#1");
				
				Assert.IsTrue (map.TryGetValue ("monkey2", out value), "#2");
				Assert.AreEqual (3, value, "#2");
				
				Assert.IsTrue (map.TryGetValue ("monkey3", out value), "#3");
				Assert.AreEqual (3, value, "#3");
				
				Assert.IsTrue (map.TryGetValue ("monkey4", out value), "#4");
				Assert.AreEqual (3, value, "#4");
			});
		}
		
		[Test]
		public void RemoveParallelTest ()
		{
			ParallelTestHelper.Repeat (delegate {
				Setup ();
				int index = 0;
				bool r1 = false, r2 = false, r3 = false;
				
				ParallelTestHelper.ParallelStressTest (map, delegate {
					int own = Interlocked.Increment (ref index);
					switch (own) {
					case 1:
						r1 = map.Remove ("foo");
						break;
					case 2:
						r2 =map.Remove ("bar");
						break;
					case 3:
						r3 = map.Remove ("foobar");
						break;
					}
				}, 3);
				
				Assert.AreEqual (0, map.Count);
				int value;
	
				Assert.IsTrue (r1, "1");
				Assert.IsTrue (r2, "2");
				Assert.IsTrue (r3, "3");
				
				Assert.IsFalse (map.TryGetValue ("foo", out value), "#1");
				Assert.IsFalse (map.TryGetValue ("bar", out value), "#2");
				Assert.IsFalse (map.TryGetValue ("foobar", out value), "#3");
			});
		}
		
		[Test, ExpectedException(typeof(ArgumentException))]
		public void AddWithDuplicate()
		{
			map.Add("foo", 6);
		}
		
		[Test]
		public void GetValueTest()
		{
			Assert.AreEqual(1, map.GetValue("foo"), "#1");
			Assert.AreEqual(2, map["bar"], "#2");
			Assert.AreEqual(3, map.Count, "#3");
		}
		
		[Test, ExpectedException(typeof(ArgumentException))]
		public void GetValueUnknownTest()
		{
			int val;
			Assert.IsFalse(map.TryGetValue("barfoo", out val));
			map.GetValue("barfoo");
		}
		
		[Test]
		public void ModificationTest()
		{
			map["foo"] = 9;
			int val;
			
			Assert.AreEqual(9, map["foo"], "#1");
			Assert.AreEqual(9, map.GetValue("foo"), "#2");
			Assert.IsTrue(map.TryGetValue("foo", out val), "#3");
			Assert.AreEqual(9, val, "#4");
		}
	}
}
//#endif
