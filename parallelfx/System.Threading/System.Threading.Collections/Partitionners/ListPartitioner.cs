// 
// ListPartitioner.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
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

using System;
using System.Linq;
using System.Collections.Generic;

namespace System.Collections.Concurrent
{
	public class ListPartitioner<T> : OrderablePartitioner<T>
	{
		IList<T> source;
		
		public ListPartitioner (IList<T> source) : base (true, true, true)
		{
			this.source = source;
		}
		
		public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions (int partitionCount)
		{
			if (partitionCount <= 0)
				throw new ArgumentOutOfRangeException ("partitionCount");
			
			IEnumerator<KeyValuePair<long, T>>[] enumerators
				= new IEnumerator<KeyValuePair<long, T>>[partitionCount];
			
			int count = (source.Count >= partitionCount) ? source.Count / partitionCount : 1;
			
			for (int i = 0; i < enumerators.Length; i++) {
				if (i != enumerators.Length - 1)
					enumerators[i] = GetEnumeratorForRange (i * count, i * count + count);
				else
					enumerators[i] = GetEnumeratorForRange (i * count, source.Count - i * count);
			}
			
			return enumerators;
		}
		
		IEnumerator<KeyValuePair<long, T>> GetEnumeratorForRange (int startIndex, int count)
		{
			if (startIndex >= source.Count)
				return Enumerable.Empty<KeyValuePair<long, T>> ().GetEnumerator ();
			
			return GetEnumeratorForRangeInternal (startIndex, count);
		}
		
		IEnumerator<KeyValuePair<long, T>> GetEnumeratorForRangeInternal (int startIndex, int count)
		{	
			for (int i = startIndex; i < count; i++) {
				yield return new KeyValuePair<long, T> (i, source[i]);
			}
		}
	}
}
