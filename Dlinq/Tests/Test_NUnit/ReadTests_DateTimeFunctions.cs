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
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Test_NUnit;


#if !MONO_STRICT
using nwind;
using DbLinq.Data.Linq;
using DataLinq = DbLinq.Data.Linq;
using DbLinq.Logging;
using System.Data.Linq;
#else
using MsNorthwind;
using System.Data.Linq;
using DataLinq = System.Data.Linq;
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
    public class ReadTests_DateTimeFunctions : TestBase
    {
        [Test]
        public void GetYear()
        {
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    where o.OrderDate.Value.Year == 1996
                    select o;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void GetMonth()
        {
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    where o.OrderDate.Value.Month == 10
                    select o;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void GetDay()
        {
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    where o.OrderDate.Value.Day == 16
                    select o;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void GetHours()
        {
            Northwind db = CreateDB();

            var q = (from o in db.Orders
                     where o.OrderDate.Value.Hour == 0
                     select o).ToList();


        }
        [Test]
        public void GetMinutes()
        {
            Northwind db = CreateDB();

            var q = (from o in db.Orders
                     where o.OrderDate.Value.Minute == 0
                     select o).ToList();


        }
        [Test]
        public void GetSeconds()
        {
            Northwind db = CreateDB();

            var q = (from o in db.Orders
                     where o.OrderDate.Value.Second == 16
                     select o).ToList();

        }

        [Test]
        public void GetMilliSeconds()
        {
            Northwind db = CreateDB();

            var q = (from o in db.Orders
                     where o.OrderDate.Value.Millisecond == 0
                     select o).ToList();

        }

        [Test]
        public void GetCurrentDateTime()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where e.BirthDate.Value == DateTime.Now
                        select e;

            var list = query.ToList();
        }

        [Test]
        public void Parse01()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where e.BirthDate.Value == DateTime.Parse("1984/05/02")
                        select e;

            var list = query.ToList();
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Parse02()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where e.BirthDate.Value == DateTime.Parse(e.BirthDate.ToString())
                        select e;

            var list = query.ToList();
        }

        [Test]
        public void Parse03()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        select e.BirthDate.Value == DateTime.Parse("1984/05/02");


            var list = query.ToList();
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Parse04()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        select e.BirthDate.Value == DateTime.Parse(e.BirthDate.ToString());


            var list = query.ToList();
        }

        [Test]
        public void DateTimeDiffTotalHours()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where (e.BirthDate.Value - DateTime.Parse("1984/05/02")).TotalHours > 0
                        select e;


            var list = query.ToList();
        }

        [Test]
        public void DateTimeDiffHours()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where (e.BirthDate.Value - DateTime.Parse("1984/05/02")).Hours > 0
                        select e;


            var list = query.ToList();
        }

        [Test]
        public void DateTimeDiffTotalMinutes()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where (e.BirthDate.Value - DateTime.Parse("1984/05/02")).TotalMinutes > 0
                        select e;


            var list = query.ToList();
        }

        [Test]
        public void DateTimeDiffMinutes()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where (e.BirthDate.Value - DateTime.Parse("1984/05/02")).Minutes > 0
                        select e;


            var list = query.ToList();
        }

        [Test]
        public void DateTimeDiffSeconds()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where (e.BirthDate.Value - DateTime.Parse("1984/05/02")).Seconds > 0
                        select e;


            var list = query.ToList();
        }

        [Test]
        public void DateTimeDiffTotalSeconds()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where (e.BirthDate.Value - DateTime.Parse("1984/05/02")).TotalSeconds > 0
                        select e;


            var list = query.ToList();
        }

        [Test]
        public void DateTimeDiffMilliseconds()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where (e.BirthDate.Value - DateTime.Parse("1984/05/02")).Milliseconds > 0
                        select e;


            var list = query.ToList();
        }

        [Test]
        public void DateTimeDiffTotalMilliseconds()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where (e.BirthDate.Value - DateTime.Parse("1984/05/02")).TotalMinutes > 0
                        select e;


            var list = query.ToList();
        }

        [Test]
        public void DateTimeDiffDays()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where (e.BirthDate.Value - DateTime.Parse("1984/05/02")).Days > 0
                        select e;


            var list = query.ToList();
        }

        [Test]
        public void DateTimeDiffTotalDaysseconds()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where (e.BirthDate.Value - DateTime.Parse("1984/05/02")).TotalDays > 0
                        select e;


            var list = query.ToList();
        }
    }
}
