// Combinators.cs
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
using System.Linq;
using System.Threading.Tasks;

namespace Mono.Threading.Actors
{
	public static class Combinators
	{
		public static Task AndThen (Action initial, params Action[] chain)
		{
			Task root = Task.StartNew (_ => initial());
			chain.Aggregate (root, (t, a) => t.ContinueWith (_ => a (),
			                                                 TaskContinuationKind.OnCompletedSuccessfully,
			                                                 TaskCreationOptions.Detached));
			
			return root;
		}

		// The following Loop and LoopWhile are to be used inside actor to simulate a infinite loop or a while-like loop. They have the same semantic but allows other Tasks to be processed and so to not clung the scheduler.
		public static Task Loop (Action body)
		{
			return AndThen (body, () => Loop(body));
		}
		
		public static Task LoopWhile (Action body, Func<bool> predicate)
		{
			if (predicate ())
				return AndThen (body, () => LoopWhile (body, predicate));
			
			return Task.StartNew (delegate { });
		}
	}
}
