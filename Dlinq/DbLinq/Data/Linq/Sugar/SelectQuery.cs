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

using System;
using System.Collections.Generic;
using System.Data;

#if MONO_STRICT
using System.Data.Linq.Sugar.Expressions;
using MappingContext = System.Data.Linq.Mapping.MappingContext;
#else
using DbLinq.Data.Linq.Sugar.Expressions;
using MappingContext = DbLinq.Data.Linq.Mapping.MappingContext;
#endif

#if MONO_STRICT
namespace System.Data.Linq.Sugar
#else
namespace DbLinq.Data.Linq.Sugar
#endif
{
    /// <summary>
    /// Represents a linq query, parsed and compiled, to be sent to database
    /// This instance is immutable, since it can be stored in a cache
    /// </summary>
    internal class SelectQuery: AbstractQuery
    {
        /// <summary>
        /// Parameters to be sent as SQL parameters
        /// </summary>
        public IList<InputParameterExpression> InputParameters { get; private set; }

        /// <summary>
        /// Expression that creates a row object
        /// Use GetRowObjectCreator() to access the object with type safety
        /// </summary>
        internal Delegate RowObjectCreator { get; private set; }

        /// <summary>
        /// Returns the row object creator, strongly typed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Func<IDataRecord, MappingContext, T> GetRowObjectCreator<T>()
        {
            return (Func<IDataRecord, MappingContext, T>)RowObjectCreator;
        }

        /// <summary>
        /// Used on scalar calls, like First()
        /// </summary>
        public string ExecuteMethodName { get; private set; }

        public SelectQuery(DataContext dataContext, string sql, IList<InputParameterExpression> parameters,
                     Delegate rowObjectCreator, string executeMethodName)
            : base(dataContext,sql)
        {
            InputParameters = parameters;
            RowObjectCreator = rowObjectCreator;
            ExecuteMethodName = executeMethodName;
        }
    }
}
