﻿#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne
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
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test_NUnit;

#if !MONO_STRICT
using nwind;
#else
using MsNorthwind;
using System.Data.Linq;
#endif

#if MYSQL
    namespace Test_NUnit_MySql
#elif ORACLE
    #if ODP
        namespace Test_NUnit_OracleODP
    #else
        namespace Test_NUnit_Oracle
    #endif
#elif POSTGRES
    namespace Test_NUnit_PostgreSql
#elif SQLITE
    namespace Test_NUnit_Sqlite
#elif INGRES
    namespace Test_NUnit_Ingres
#elif MSSQL
#if MONO_STRICT
    namespace Test_NUnit_MsSql_Strict
#else
    namespace Test_NUnit_MsSql
#endif
#else
    #error unknown target
#endif
{
    [TestFixture]
    public class ExecuteCommand_Test : TestBase
    {
        [Test]
        public void A2_ProductsTableHasEntries()
        {
            Northwind db = CreateDB();
            //string sql = "SELECT count(*) FROM Northwind.Products";
            int result = db.ExecuteCommand("SELECT count(*) FROM Products");
            //long iResult = base.ExecuteScalar(sql);
            Assert.Greater(result, 0, "Expecting some rows in Products table, got:" + result);
        }

        /// <summary>
        /// like above, but includes one parameter.
        /// </summary>
        [Test]
        public void A3_ProductCount_Param()
        {
            Northwind db = CreateDB();
            int result = db.ExecuteCommand("SELECT count(*) FROM [Products] WHERE [ProductID]>{0}", 3);
            //long iResult = base.ExecuteScalar(sql);
            Assert.Greater(result, 0, "Expecting some rows in Products table, got:" + result);
        }

    }
}
