﻿#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
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
#endregion

using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DbLinq.Util;
using System.Collections.Generic;

namespace DbLinq.Vendor.Implementation
{
    partial class Vendor
    {
        /// <summary>
        /// holds result of a stored proc call.
        /// </summary>
#if MONO_STRICT
        internal
#else
        public
#endif
        class ProcedureResult : System.Data.Linq.IExecuteResult
        {
            object[] outParamValues;

            public object GetParameterValue(int parameterIndex)
            {
                object value = outParamValues[parameterIndex];
                return value;
            }

            public object ReturnValue { get; set; }

            public void Dispose() { }

            public ProcedureResult(object retVal, object[] outParamValues_)
            {
                ReturnValue = retVal;
                outParamValues = outParamValues_;
            }
        }
    }
}
