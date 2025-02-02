// ParallelEnumerable.cs
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
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using PEHelper = System.Linq.ParallelEnumerableHelper;

namespace System.Linq
{
	public static class ParallelEnumerable
	{
		
		#region Select
		public static IParallelEnumerable<TResult> Select<TSource, TResult>(this IParallelEnumerable<TSource> source,
		                                                                    Func<TSource, TResult> selector)
		{
			return Select (source, (TSource e, int index) => selector(e));
		}
		
		public static IParallelEnumerable<TResult> Select<TSource, TResult>(this IParallelEnumerable<TSource> source,
		                                                                    Func<TSource, int, TResult> selector)
		{
			return PEHelper.Process<TSource, TResult> (source, delegate (int i, TSource e) {
				return new ResultReturn<TResult>(true, true, selector(e, i), i);
			});
		}
		#endregion
		
		#region Where
		public static IParallelEnumerable<TSource> Where<TSource>(this IParallelEnumerable<TSource> source,
		                                                          Func<TSource, bool> predicate)
		{
			return Where(source, (TSource e, int index) => predicate(e));
		}
		
		public static IParallelEnumerable<TSource> Where<TSource>(this IParallelEnumerable<TSource> source,
		                                                          Func<TSource, int, bool> predicate)
		{
			return PEHelper.Process<TSource, TSource> (source, delegate (int i, TSource e) {
				if (predicate(e, i))
					return new ResultReturn<TSource>(true, true, e, i);
				else
					return new ResultReturn<TSource>(true, false, default(TSource), i);
			});
		}
		#endregion
		
		#region Count
		public static int Count<TSource>(this IParallelEnumerable<TSource> source)
		{
			var coll = source.AsICollection();
			if (coll != null) return coll.Count;
			
			return source.Aggregate<TSource, int, int>(() => 0,
			                                           (acc, e) => acc + 1,
			                                           (acc1, acc2) => acc1 + acc2,
			                                           (result) => result);
		}
		
		public static int Count<TSource>(this IParallelEnumerable<TSource> source, Func<TSource, bool> predicate)
		{	
			var coll = source.AsICollection();
			if (coll != null) return coll.Count;
			
			return source.Aggregate<TSource, int, int>(() => 0,
			                                           (acc, e) => predicate (e) ? acc + 1 : acc,
			                                           (acc1, acc2) => acc1 + acc2,
			                                           (result) => result);
		}
		#endregion
		
		#region Any
		public static bool Any<TSource>(this IParallelEnumerable<TSource> source)
		{
			// Little short-circuit
			var coll = source.AsICollection();
			if (coll != null) return coll.Count > 0;
			
			bool result = source.GetParallelEnumerator(false).MoveNext();
			
			return result;
		}
		#endregion
		
		#region ForAll
		public static void ForAll<T>(this IParallelEnumerable<T> source, Action<T> action)
		{
			PEHelper.Process(source, (i, e) => { action(e); }, true);
		}
		#endregion
		
		#region
		public static IParallelEnumerable<T> Reverse<T>(this IParallelEnumerable<T> source)
		{
			// HACK
			List<T> temp = source.AsOrdered().ToList();
			temp.Reverse();
			return ParallelEnumerableFactory.GetFromIEnumerable(temp, source.Dop());
		}
		#endregion
		
		#region OrderBy
		public static IParallelOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IParallelEnumerable<TSource> source,
		                                                                         Func<TSource, TKey> keySelector)
		{
			return source.OrderBy(keySelector, Comparer<TKey>.Default);
		}
		
		public static IParallelOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IParallelEnumerable<TSource> source,
		                                                                         Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");
			if (keySelector == null)
				throw new ArgumentNullException("keySelector");
			
			return OrderByInternal<TSource>(source, (e1, e2) => {
				if (e1 == null || e2 == null)
					return 0;
				return comparer.Compare(keySelector(e1), keySelector(e2));
			});
		}
		
		public static IParallelOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IParallelEnumerable<TSource> source,
		                                                                                   Func<TSource, TKey> keySelector)
		{
			return source.OrderByDescending(keySelector, Comparer<TKey>.Default);
		}
		
		public static IParallelOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IParallelEnumerable<TSource> source,
		                                                                  Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");
			return OrderByInternal<TSource>(source, (e1, e2) => comparer.Compare(keySelector(e2), keySelector(e1)));
		}
		
		static IParallelOrderedEnumerable<TSource> OrderByInternal<TSource>(IParallelEnumerable<TSource> source,
		                                                           System.Comparison<TSource> comparison)
		{
			return ParallelEnumerableFactory.GetOrdered(source, comparison);
		}
		#endregion
		
		#region ThenBy
		public static IParallelOrderedEnumerable<T> ThenBy<T, TKey> (this IParallelOrderedEnumerable<T> source,
		                                                             Func<T, TKey> keySelector)
		{
			return source.ThenBy(keySelector, Comparer<TKey>.Default);
		}
		
		public static IParallelOrderedEnumerable<T> ThenBy<T, TKey> (this IParallelOrderedEnumerable<T> source,
		                                                             Func<T, TKey> keySelector,
		                                                             IComparer<TKey> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");
			
			return source.CreateParallelOrderedEnumerable<TKey>(keySelector, comparer, false);
		}
		
		public static IParallelOrderedEnumerable<T> ThenByDescending<T, TKey> (this IParallelOrderedEnumerable<T> source,
		                                                                       Func<T, TKey> keySelector)
		{
			return source.ThenByDescending(keySelector, Comparer<TKey>.Default);
		}
		
		public static IParallelOrderedEnumerable<T> ThenByDescending<T, TKey> (this IParallelOrderedEnumerable<T> source,
		                                                                       Func<T, TKey> keySelector,
		                                                                       IComparer<TKey> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");
			
			return source.CreateParallelOrderedEnumerable<TKey>(keySelector, comparer, true);
		}
		#endregion 
		
		#region Average
		public static double Average(this IParallelEnumerable<int> source)
		{
			return source.Aggregate(() => new int[2],
			                        (acc, e) => { acc[0] += e; acc[1]++; return acc; },
			                        (acc1, acc2) => { acc1[0] += acc2[0]; acc1[1] += acc2[1]; return acc1; },
			                        (acc) => acc[0] / ((double)acc[1]));
		}
		
		public static double Average(this IParallelEnumerable<long> source)
		{
			return source.Aggregate(() => new long[2],
			                        (acc, e) => { acc[0] += e; acc[1]++; return acc; },
			                        (acc1, acc2) => { acc1[0] += acc2[0]; acc1[1] += acc2[1]; return acc1; },
			                        (acc) => acc[0] / ((double)acc[1]));
		}
		#endregion
		
		#region Sum
		public static int Sum(IParallelEnumerable<int> source)
		{
			return source.Aggregate(0, (e1, e2) => e1 + e2, (sum1, sum2) => sum1 + sum2, (sum) => sum);
		}
		
		public static byte Sum(IParallelEnumerable<byte> source)
		{
			return source.Aggregate((byte)0, (e1, e2) => (byte)(e1 + e2), (sum1, sum2) => (byte)(sum1 + sum2), (sum) => sum);
		}
		
		public static short Sum(IParallelEnumerable<short> source)
		{
			return source.Aggregate((short)0, (e1, e2) => (short)(e1 + e2), (sum1, sum2) => (short)(sum1 + sum2), (sum) => sum);
		}
		
		public static long Sum(IParallelEnumerable<long> source)
		{
			return source.Aggregate((long)0, (e1, e2) => e1 + e2, (sum1, sum2) => sum1 + sum2, (sum) => sum);
		}
		
		public static float Sum(IParallelEnumerable<float> source)
		{
			return source.Aggregate(0.0f, (e1, e2) => e1 + e2, (sum1, sum2) => sum1 + sum2, (sum) => sum);
		}
		
		public static double Sum(IParallelEnumerable<double> source)
		{
			return source.Aggregate(0.0, (e1, e2) => e1 + e2, (sum1, sum2) => sum1 + sum2, (sum) => sum);
		}
		
		public static decimal Sum(IParallelEnumerable<decimal> source)
		{
			return source.Aggregate((decimal)0, (e1, e2) => e1 + e2, (sum1, sum2) => sum1 + sum2, (sum) => sum);
		}
		#endregion

		#region Sum (nullable)
		/*public static int Sum(IParallelEnumerable<int?> source)
		{
			return source.Where(PEHelper.NullableExistencePredicate<int>).Select<int?, int>(NullableExtractor<int>).Sum();
		}
		
		public static byte Sum(IParallelEnumerable<byte?> source)
		{
			return Sum(source.Where(PEHelper.NullableExistencePredicate<byte>).Select<byte?, byte>(NullableExtractor<byte>));
		}*/
		
		/*public static short Sum(IParallelEnumerable<short?> source)
		{
			return source.Where(NullableExistencePredicate<short>).Select<int?, int>(NullableExtractor<short>).Sum();
		}*/
		
		/*public static long Sum(IParallelEnumerable<long?> source)
		{
			return source.Where(PEHelper.NullableExistencePredicate<long>).Select<long?, long>(NullableExtractor<long>).Sum();
		}
		
		public static float Sum(IParallelEnumerable<float?> source)
		{
			return source.Where(PEHelper.NullableExistencePredicate<float>).Select<float?, float>(NullableExtractor<float>).Sum();
		}
		
		public static double Sum(IParallelEnumerable<double?> source)
		{
			return source.Where(PEHelper.NullableExistencePredicate<double>).Select<double?, double>(NullableExtractor<double>).Sum();
		}
		
		public static decimal Sum(IParallelEnumerable<decimal?> source)
		{
			return source.Where(PEHelper.NullableExistencePredicate<decimal>).Select<decimal?, decimal>(NullableExtractor<decimal>).Sum();
		}*/
		#endregion
		
		#region Min - Max
		public static int Min(this IParallelEnumerable<int> source)
		{
			return PEHelper.BestOrder(source, (first, second) => first < second, int.MaxValue);
		}
		
		public static byte Min(this IParallelEnumerable<byte> source)
		{
			return PEHelper.BestOrder(source, (first, second) => first < second, byte.MaxValue);
		}
		
		public static short Min(this IParallelEnumerable<short> source)
		{
			return PEHelper.BestOrder(source, (first, second) => first < second, short.MaxValue);
		}
		
		public static long Min(this IParallelEnumerable<long> source)
		{
			return PEHelper.BestOrder(source, (first, second) => first < second, long.MaxValue);
		}
		
		public static float Min(this IParallelEnumerable<float> source)
		{
			return PEHelper.BestOrder(source, (first, second) => first < second, float.MaxValue);
		}
		
		public static double Min(this IParallelEnumerable<double> source)
		{
			return PEHelper.BestOrder(source, (first, second) => first < second, double.MaxValue);
		}
		
		public static decimal Min(this IParallelEnumerable<decimal> source)
		{
			return PEHelper.BestOrder(source, (first, second) => first < second, decimal.MaxValue);
		}
		
		public static int Max(this IParallelEnumerable<int> source)
		{
			return PEHelper.BestOrder(source, (first, second) => first > second, int.MinValue);
		}
		
		public static byte Max(this IParallelEnumerable<byte> source)
		{
			return PEHelper.BestOrder(source, (first, second) => first > second, byte.MinValue);
		}
		
		public static short Max(this IParallelEnumerable<short> source)
		{
			return PEHelper.BestOrder(source, (first, second) => first > second, short.MinValue);
		}
		
		public static long Max(this IParallelEnumerable<long> source)
		{
			return PEHelper.BestOrder(source, (first, second) => first > second, long.MinValue);
		}
		
		public static float Max(this IParallelEnumerable<float> source)
		{
			return PEHelper.BestOrder(source, (first, second) => first > second, float.MinValue);
		}
		
		public static double Max(this IParallelEnumerable<double> source)
		{
			return PEHelper.BestOrder(source, (first, second) => first > second, double.MinValue);
		}
		
		public static decimal Max(this IParallelEnumerable<decimal> source)
		{
			return PEHelper.BestOrder(source, (first, second) => first > second, decimal.MinValue);
		}
		#endregion

		
		#region Min - Max (Nullable)
		
		/*public static int Min(this IParallelEnumerable<int?> source)
		{
			return source.Where(NullableExistencePredicate<int>).Select(NullableExtractor<int>).Min();
		}
		
		public static byte Min(this IParallelEnumerable<byte?> source)
		{
			return source.Where(NullableExistencePredicate<byte>).Min();
		}
		
		public static short Min(this IParallelEnumerable<short?> source)
		{
			return source.Where(NullableExistencePredicate<short>).Min();
		}
		
		public static long Min(this IParallelEnumerable<long?> source)
		{
			return source.Where(NullableExistencePredicate<long>).Min();
		}
		
		public static float Min(this IParallelEnumerable<float?> source)
		{
			return source.Where(NullableExistencePredicate<float>).Min();
		}
		
		public static double Min(this IParallelEnumerable<double?> source)
		{
			return source.Where(NullableExistencePredicate<double>).Min();
		}
		
		public static decimal Min(this IParallelEnumerable<decimal?> source)
		{
			return source.Where(NullableExistencePredicate<decimal>).Min();
		}
		
		public static int Max(this IParallelEnumerable<int?> source)
		{
			return source.Where(NullableExistencePredicate<int>).Max();
		}
		
		public static byte Max(this IParallelEnumerable<byte?> source)
		{
			return source.Where(NullableExistencePredicate<byte>).Max();
		}
		
		public static short Max(this IParallelEnumerable<short?> source)
		{
			return source.Where(NullableExistencePredicate<short>).Max();
		}
		
		public static long Max(this IParallelEnumerable<long?> source)
		{
			return source.Where(NullableExistencePredicate<long>).Max();
		}
		
		public static float Max(this IParallelEnumerable<float?> source)
		{
			return source.Where(NullableExistencePredicate<float>).Max();
		}
		
		public static double Max(this IParallelEnumerable<double?> source)
		{
			return source.Where(NullableExistencePredicate<double>).Max();
		}
		
		public static decimal Max(this IParallelEnumerable<decimal?> source)
		{
			return source.Where(NullableExistencePredicate<decimal>).Max();
		}*/
		#endregion
		
		#region Aggregate
		public static TResult Aggregate<TSource, TAccumulate, TResult>(this IParallelEnumerable<TSource> source,
		                                                               TAccumulate seed,
		                                                               Func<TAccumulate, TSource, TAccumulate> intermediateReduceFunc,
		                                                               Func<TAccumulate, TAccumulate, TAccumulate> finalReduceFunc,
		                                                               Func<TAccumulate, TResult> resultSelector)
		{
			return source.Aggregate(() => seed, intermediateReduceFunc, finalReduceFunc, resultSelector);
		}
		
		public static TResult Aggregate<TSource, TAccumulate, TResult>(this IParallelEnumerable<TSource> source,
		                                                               Func<TAccumulate> seedFactory,
		                                                               Func<TAccumulate, TSource, TAccumulate> intermediateReduceFunc,
		                                                               Func<TAccumulate, TAccumulate, TAccumulate> finalReduceFunc,
		                                                               Func<TAccumulate, TResult> resultSelector)
		{
			int count = Parallel.GetBestWorkerNumber();
			
			TAccumulate[] accumulators = new TAccumulate[count];
			
			for (int i = 0; i < count; i++) {
				accumulators[i] = seedFactory();
			}

			IParallelEnumerator<TSource> feedEnum = source.GetParallelEnumerator(false);

			// Process to intermediate result into distinct domain
			// Still hackish in the sense that it's not wrapped, hovewer remove the overhead of Interlocked
			Task[] tasks = new Task[count];
			for (int i = 0; i < count; i++) {
				int reserved = i;
				tasks[i] = Task.Factory.StartNew(delegate {
					TSource item;
					int index;
					
					while (feedEnum.MoveNext(out item, out index)) {
						accumulators[reserved] = intermediateReduceFunc(accumulators[reserved], item);
					}
					
				});
			}
			Task.WaitAll(tasks);
			
			// Reduce the final domains into a single one
			for (int i = 1; i < count; i++) {
				accumulators[0] = finalReduceFunc(accumulators[0], accumulators[i]);
			}
			// Return the final result
			return resultSelector(accumulators[0]);
		}
		#endregion
		
		#region Concat
		public static IParallelEnumerable<TSource> Concat<TSource>(this IParallelEnumerable<TSource> source,
		                                                           IParallelEnumerable<TSource> second)
		{	
			IParallelEnumerable<TSource> temp = 
				ParallelEnumerableFactory.GetFromIParallelEnumerable(source.Dop(), source, second);
			
			return PEHelper.Process<TSource, TSource>(temp, delegate (int i, TSource e) {
				return new ResultReturn<TSource>(true, true, e, i);
			});
			
		}
		#endregion
		
		#region Take
		public static IParallelEnumerable<TSource> Take<TSource>(this IParallelEnumerable<TSource> source, int count)
		{
			int counter = 0;
			
			return PEHelper.Process<TSource, TSource> (source.AsOrdered(), delegate (int i, TSource e) {
				if (Interlocked.Increment(ref counter) <= count) {
					return new ResultReturn<TSource>(true, true, e, i);
				} else {
					return ResultReturn<TSource>.False;
				}
			});
		}
		
		public static IParallelEnumerable<TSource> TakeWhile<TSource>(this IParallelEnumerable<TSource> source, 
		                                                              Func<TSource, bool> predicate)
		{
			return source.TakeWhile((e, i) => predicate(e));
		}
		
		public static IParallelEnumerable<TSource> TakeWhile<TSource>(this IParallelEnumerable<TSource> source, 
		                                                              Func<TSource, int, bool> predicate)
		{
			bool stopFlag = true;
			
			return PEHelper.Process<TSource, TSource> (source.AsOrdered(), delegate (int i, TSource e) {
				if (stopFlag && (stopFlag = predicate(e, i))) {
					return new ResultReturn<TSource>(true, true, e, i);
				} else {
					return ResultReturn<TSource>.False;
				}
			});
		}
		#endregion
		
		#region Skip
		public static IParallelEnumerable<TSource> Skip<TSource>(this IParallelEnumerable<TSource> source, int count)
		{
			int counter = 0;
			
			return source.AsOrdered().Where((element, index) => Interlocked.Increment(ref counter) > count);
		}
		
		public static IParallelEnumerable<TSource> SkipWhile<TSource>(this IParallelEnumerable<TSource> source, 
		                                                              Func<TSource, bool> predicate)
		{
			return source.SkipWhile((e, i) => predicate(e));
		}
		
		public static IParallelEnumerable<TSource> SkipWhile<TSource>(this IParallelEnumerable<TSource> source, 
		                                                              Func<TSource, int, bool> predicate)
		{
			bool predicateStatus = true;
			
			return source.AsOrdered().Where((element, index) => {
				if (!predicateStatus)
					return true;
				
				bool result = predicate(element, index);
				if (!result)
					predicateStatus = true;
				return !result;
			});
		}
		#endregion
		
		#region ElementAt
		public static TSource ElementAt<TSource>(this IParallelEnumerable<TSource> source, int index)
		{
			// Little short-circuit taken from Enumerable.cs
			var list = source.AsIList();
			if (list != null) return list[index];
			
			TSource result = default(TSource);
			int currIndex = -1;
			
			PEHelper.Process(source.AsOrdered(), delegate (int j, TSource element) {
				int myIndex = Interlocked.Increment(ref currIndex);
				if (myIndex == index) {
					result = element;
					return false;
				}
				return true;
			}, true);
			
			return result;
		}
		#endregion
		
		#region First
		public static TSource First<TSource>(this IParallelEnumerable<TSource> source)
		{
			PEHelper.AssertSourceNotNull(source);
			
			// Little short-circuit taken from Enumerable.cs
			var list = source.AsIList();
			if (list != null) {
				if (list.Count > 0)
					return list [0];
				else
					throw new InvalidOperationException("source is empty");
			}
			
			TSource first;
			int index;
			
			bool result = source.GetParallelEnumerator(false).MoveNext(out first, out index);
			
			if (!result)
				throw new InvalidOperationException("source is empty");
			
			return first;
		}
		
		public static TSource First<TSource>(this IParallelEnumerable<TSource> source,
		                                     Func<TSource, bool> predicate)
		{
			PEHelper.AssertSourceNotNull(source);
			
			TSource element = default(TSource);
			
			bool result = false;
			
			PEHelper.Process<TSource>(source.AsOrdered (), delegate (int i, TSource e) {
				if (predicate(e)) {
					element = e;
					result = true;
					return false;
				} else {
					return true;
				}
			}, true);
			
			PEHelper.Assert(result, (r) => !r, () => new Exception("No element found"));
			
			return element;
		}
		
		public static TSource FirstOrDefault<TSource>(this IParallelEnumerable<TSource> source)
		{
			PEHelper.AssertSourceNotNull(source);
			
			// Little short-circuit taken from Enumerable.cs
			var list = source.AsIList();
			if (list != null)
				return (list.Count > 0) ? list [0] : default(TSource);
			
			TSource first;
			int index;
			
			bool result = source.GetParallelEnumerator(false).MoveNext(out first, out index);
			
			return result ? first : default(TSource);
		}
		
		public static TSource FirstOrDefault<TSource>(this IParallelEnumerable<TSource> source,
		                                              Func<TSource, bool> predicate)
		{
			PEHelper.AssertSourceNotNull(source);
			
			TSource element = default(TSource);
			
			PEHelper.Process<TSource>(source.AsOrdered (), delegate (int i, TSource e) {
				if (predicate(e)) {
					element = e;
					return false;
				} else {
					return true;
				}
			}, true);
			
			return element;
		}
		#endregion
	
		#region
		public static IParallelEnumerable<T> DefaultIfEmpty<T>(this IParallelEnumerable<T> source)
		{
			return source.DefaultIfEmpty(default(T));
		}
		
		public static IParallelEnumerable<T> DefaultIfEmpty<T>(this IParallelEnumerable<T> source, T defValue)
		{
			if (source.Any())
				return source;
			else
				return ParallelEnumerable.Repeat(defValue, 1);
		}
		#endregion
		
		#region ToArray - ToList
		// This ones is the most efficient, however it brokes ordering in all case
		public static List<T> ToList<T>(this IParallelEnumerable<T> source)
		{
			/*var ordered = source as IParallelOrderedEnumerable<T>;
			if (ordered != null)
				return ToList (ordered);*/
			
			List<T> temp = source.Aggregate(() => new List<T>(),
			                                (list, e) => { list.Add(e); return list; },
			                                (list, list2) => { list.AddRange(list2); return list; },
			                                (list) => list);
			return temp;
		}
	
		public static List<T> ToList<T>(this IParallelOrderedEnumerable<T> source)
		{
			List<T> temp = new List<T>();
			foreach (T e in source)
				temp.Add (e);
			
			return temp;
		}
		
		public static T[] ToArray<T>(this IParallelEnumerable<T> source)
		{
			return source.ToList().ToArray();	
		}
		
		public static T[] ToArray<T>(this IParallelOrderedEnumerable<T> source)
		{
			return source.ToList().ToArray();	
		}
		#endregion
		
		#region Zip
		public static IParallelEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IParallelEnumerable<TFirst> first,
		                                                                         IEnumerable<TSecond> second,
		                                                                         Func<TFirst, TSecond, TResult> resultSelector)
		{			
			IParallelEnumerable<TSecond> pEnumerable = 
				second as IParallelEnumerable<TSecond> ?? ParallelEnumerableFactory.GetFromIEnumerable(second, first.Dop());
			IParallelEnumerator<TSecond> pEnum = pEnumerable.GetParallelEnumerator(false);

			return PEHelper.Process(first, (index, element) => {
				TSecond element2;
				int i;
				
				if (pEnum.MoveNext(out element2, out i))
					return new ResultReturn<TResult>(true, true, resultSelector(element, element2), index);
				else
					return ResultReturn<TResult>.False; 
			});
		}
		#endregion
		
		#region Range & Repeat
		public static IParallelEnumerable<int> Range(int start, int count)
		{
			return ParallelEnumerableFactory.GetFromRange (start, count, PEHelper.DefaultDop);
		}
		
		public static IParallelEnumerable<TResult> Repeat<TResult>(TResult element, int count)
		{
			return ParallelEnumerableFactory.GetFromRepeat<TResult>(element, count, PEHelper.DefaultDop);
		}
		#endregion
	}
}
