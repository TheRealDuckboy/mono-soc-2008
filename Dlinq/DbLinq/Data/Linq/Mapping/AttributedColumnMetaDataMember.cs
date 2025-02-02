﻿#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry
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

using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;
using DbLinq.Util;

#if MONO_STRICT
namespace System.Data.Linq.Mapping
#else
namespace DbLinq.Data.Linq.Mapping
#endif
{
    [DebuggerDisplay("MetaDataMember for {MappedName}")]
    internal class AttributedColumnMetaDataMember : AttributedAbstractMetaDataMember
    {
        public AttributedColumnMetaDataMember(MemberInfo member, ColumnAttribute attribute, MetaType declaringType)
            : base(member, declaringType, member.DeclaringType.GetSingleMember(attribute.Storage))
        {
            columnAttribute = attribute;
            if (columnAttribute.Name == null)
                columnAttribute.Name = memberInfo.Name;
        }

        private ColumnAttribute columnAttribute;

        public override MetaAssociation Association
        {
            get { return null; }
        }

        public override AutoSync AutoSync
        {
            get { return columnAttribute.AutoSync; }
        }

        public override string DbType
        {
            get { return columnAttribute.DbType; }
        }

        public override string Expression
        {
            get { return columnAttribute.Expression; }
        }

        public override bool IsAssociation
        {
            get { return false; }
        }

        public override bool IsDbGenerated
        {
            get { return columnAttribute.IsDbGenerated; }
        }

        public override bool IsDiscriminator
        {
            get { return columnAttribute.IsDiscriminator; }
        }

        public override bool IsPrimaryKey
        {
            get { return columnAttribute.IsPrimaryKey; }
        }

        public override bool IsVersion
        {
            get { return columnAttribute.IsVersion; }
        }

        public override string MappedName
        {
            get { return columnAttribute.Name ?? Member.Name; }
        }
    }
}