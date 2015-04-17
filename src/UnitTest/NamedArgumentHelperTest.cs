//The MIT License (MIT)
//
//Copyright 2015 Microsoft Corporation
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.
//
﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using EnhancedMonitoring;
using EnhancedMonitoring.Configuration;

namespace UnitTest
{
    [TestClass]
    public class NamedArgumentHelperTest
    {
        [TestMethod]
        public void TestResolveNamedArgument()
        {
            var str = "MaxClockSpeed_{0}_{1}";
            var expected = "MaxClockSpeed_ubuntu_0";
            var argsName = new List<WhereArgConfiguration>(){
                new WhereArgConfiguration(){ Name ="VirtualMachine"},  
                new WhereArgConfiguration(){Name = "ProcessorId" }
            };
            var args = new Dictionary<String, Object>()
            {
                {"VirtualMachine", "ubuntu"},
                {"ProcessorId", 0}
            };

            Assert.AreEqual(expected, NamedArgumentHelper.Resolve(str, argsName, args));
        }

        [TestMethod]
        public void TestResolveNamedArgumentNagativeCase()
        {  
            var str = "MaxClockSpeed_{0}_{1}";
            var argsName = new List<WhereArgConfiguration>(){
                new WhereArgConfiguration(){ Name ="VirtualMachine"},  
                new WhereArgConfiguration(){Name = "ProcessorId" }
            };
            var args = new Dictionary<String, Object>(){ {"VirtualMachine", "ubuntu"}};

            //Argument name is null
            try
            {
                NamedArgumentHelper.Resolve(str, null, args);
                Assert.Fail("FormatException is expected");
            }
            catch (ArgumentException e) 
            { 
                Console.WriteLine(e); 
            }

            //Argument names are less than cells
            try
            {
                NamedArgumentHelper.Resolve(str, new List<WhereArgConfiguration>() { 
                    new WhereArgConfiguration(){Name = "VirtualMachine"} }, args);
                Assert.Fail("FormatException is expected");
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
            }

            //Name argument map is null
            try
            {
                NamedArgumentHelper.Resolve(str, argsName, null);
                Assert.Fail("ArgumentException is expected");
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
            }

            //Named argument not found
            try
            {
                NamedArgumentHelper.Resolve(str, argsName, args);
                Assert.Fail("ArgumentException is expected");
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
