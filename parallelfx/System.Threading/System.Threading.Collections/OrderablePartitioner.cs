// 
// OrderablePartitioner.cs
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
using System.Collections.Generic;

namespace System.Collections.Concurrent
{
	public abstract class OrderablePartitioner<T> : Partitioner<T>
	{
		bool keysOrderedInEachPartition;
		bool keysOrderedAcrossPartitions;
		bool keysNormalized;
		
		protected OrderablePartitioner (bool keysOrderedInEachPartition,
		                                bool keysOrderedAcrossPartitions, 
		                                bool keysNormalized) : base ()
		{
			this.keysOrderedInEachPartition = keysOrderedInEachPartition;
			this.keysOrderedAcrossPartitions = keysOrderedAcrossPartitions;
			this.keysNormalized = keysNormalized;
		}
		
		public abstract override IEnumerable<T> GetDynamicPartitions ();
		
		public abstract override IList<IEnumerator<T>> GetPartitions (int partitionCount);		
		
		public bool KeysOrderedInEachPartition {
			get {
				return keysOrderedInEachPartition;
			}
		}
		
		public bool KeysOrderedAcrossPartitions {
			get {
				return keysOrderedAcrossPartitions;
			}
		}
		
		public bool KeysNormalized {
			get {
				return keysNormalized;
			}
		}
	}
}
