// PEBlockingCollectionEnumerators.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Threading.Collections;

namespace System.Linq
{
	#region BlockingCollectionEnumeratorBase
	internal abstract class BlockingCollectionEnumeratorBase<T>: IParallelEnumerator<T>
	{
		protected T current;
			
		T IEnumerator<T>.Current {
			get {
				return current;
			}
		}
		
		object IEnumerator.Current {
			get {
				return (object) current;
			}
		}
			
		public abstract bool MoveNext();
		
		public abstract bool MoveNext(out T item, out int index);
		
		public virtual void Reset()
		{
		}
		
		public virtual void Dispose()
		{
		}
	}
	#endregion
		
	#region BlockingCollectionOrderedEnumerator
	internal class BlockingCollectionOrderedEnumerator<TSource, T>: BlockingCollectionEnumeratorBase<T>
	{
		/*struct Tuple<U, V>
		{
			public readonly U First;
			public readonly V Second;
			
			public Tuple(U first, V second)
			{
				First = first;
				Second = second;
			}
		}*/
		
		SpinLock sl = new SpinLock(false);
		readonly SpinWait sw = new SpinWait();
		readonly Func<IParallelEnumerator<TSource>, Action<T, bool, int>, bool> action;
		readonly IParallelEnumerator<TSource> enumerator;
		// first element of the tuple is the index and the second is the element of the IEnumerator
		//readonly BlockingCollection<Tuple<int, T>> buffer = new BlockingCollection<Tuple<int, T>>();

		int  currIndex = -1;
		
		public BlockingCollectionOrderedEnumerator(Func<IParallelEnumerator<TSource>, Action<T, bool, int>, bool> action,
		                                           IParallelEnumerator<TSource> enumerator)
		{
			this.action = action;
			this.enumerator = enumerator;
		}
		
		public override bool MoveNext()
		{
			int index;
			return MoveNext(out current, out index);
		}
		
		public override bool MoveNext(out T item, out int index)
		{
			T privElement = default(T);
			int i = index = -1;
			bool isValid = false;
			bool result  = false;
			Action<T, bool, int> adder = delegate (T e, bool v, int ind) {
				if (isValid = v) {
					privElement = e;
					i = ind;
				}
			};
			
			result = action(enumerator, adder);
				
			bool hasGoodValue = false;
			while (!hasGoodValue && result) {
				try {
					sl.Enter();
					if (result && i == currIndex + 1) {
						// If we have the right element index-speaking then everything is fine
						currIndex++;
						// If the element is invalid we just udpate the currIndex and fetch a new element
						if (isValid) {
							current = item  = privElement;
							index = i;
							hasGoodValue = true;
						} else {
							result = action(enumerator, adder);
						}
					}
				} finally {
					sl.Exit();
				}
				if (!hasGoodValue)
					sw.SpinOnce();
			}
			
			return result;
		}
	}
	#endregion

	#region not last Enumerator
	internal class BlockingCollectionEnumerator<TSource, T>: BlockingCollectionEnumeratorBase<T>
	{
		readonly Func<IParallelEnumerator<TSource>, Action<T, bool, int>, bool> action;
		readonly IParallelEnumerator<TSource> enumerator;
		
		bool isValid;
		
		public BlockingCollectionEnumerator(Func<IParallelEnumerator<TSource>, Action<T, bool, int>, bool> action,
		                                    IParallelEnumerator<TSource> enumerator)
		{
			this.action = action;
			this.enumerator = enumerator;
		}
		
		void CurrentAdder(T element, bool isValid, int index)
		{
			if (this.isValid = isValid)
				current = element;
		}
		
		public override bool MoveNext()
		{
			bool result;
			do {
				result = action(enumerator, CurrentAdder);
			} while (!isValid && result);
			
			return result;
		}
		
		public override bool MoveNext(out T item, out int index)
		{
			T privElement = default(T);
			int i = -1;
			bool isValid = false;
			bool result  = false;
			Action<T, bool, int> adder = delegate (T e, bool v, int ind) {
				if (isValid = v) {
					privElement = e;
					i = ind;
				}
			};
			
			do {
				result = action(enumerator, adder);
			} while(!isValid && result);
			
			current = item  = privElement;
			index = i;
			
			return result;
		}
	}
	#endregion
	
	#region IsLastEnumerator
	internal class BlockingCollectionIsLastEnumerator<T>: BlockingCollectionEnumeratorBase<T>
	{
		readonly BlockingCollection<T> bColl;
		
		public BlockingCollectionIsLastEnumerator(BlockingCollection<T> bColl)
		{
			this.bColl =  bColl;
		}
		
		public override bool MoveNext()
		{
			if (bColl.IsCompleted)
				return false;
			
			try {
				current = bColl.Remove();
			} catch {
				return false;
			}
			
			return true;
		}
		
		public override bool MoveNext(out T item, out int index)
		{
			index = -1;
			item = default(T);
			
			if (bColl.IsCompleted)
				return false;
			
			try {
				item = bColl.Remove();
			} catch {
				item = default(T);
				return false;
			}
			
			current = item;
			
			return true;
		}
	}
	#endregion
	
}
