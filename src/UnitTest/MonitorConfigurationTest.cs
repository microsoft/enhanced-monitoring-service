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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnhancedMonitoring.Configuration;

namespace UnitTest
{
    [TestClass]
    public class MonitorConfigurationTest
    {
        [TestMethod]
        public void TestLoadConfiguration()
        {
            var conf = MonitorConfiguration.Load(@"..\..\SampleMonitor.xml");

            Assert.IsNotNull(conf);
            Assert.IsNotNull(conf.Version);
            Console.WriteLine(conf.Version);
            Assert.AreEqual(60, conf.RefreshRate);
            Assert.AreEqual(700, conf.MaxValueLength);
            Assert.IsNotNull(conf.Kvp);
            Assert.AreEqual(200, conf.Kvp.WriteInterval);
            Assert.IsTrue(conf.Kvp.BatchMode);

            Assert.IsNotNull(conf.SupportedVMDetector); 
            Assert.IsNotNull(conf.SupportedVMDetector.GuestDataItemKey);

            Assert.IsNotNull(conf.MgmtObjects);
            var mgmtObj = conf.MgmtObjects.FirstOrDefault();
            Assert.IsNotNull(mgmtObj);
            Assert.IsTrue(mgmtObj.SuppressError);
            Assert.IsNotNull(mgmtObj.Namespace);
            Assert.IsNotNull(mgmtObj.Where);
            Assert.IsNotNull(mgmtObj.From);
            Assert.IsNotNull(mgmtObj.WhereArgs);
            Assert.IsTrue(mgmtObj.WhereArgs.FirstOrDefault().Escape);
            Assert.IsNotNull(mgmtObj.WhereArgs.FirstOrDefault());

            Assert.IsNotNull(mgmtObj.PerfCounters);
            var counter = mgmtObj.PerfCounters.FirstOrDefault();
            Assert.IsNotNull(counter);
            Assert.IsNotNull(counter.Select);
            Assert.IsNotNull(counter.As);

            var dynamicMemMgmtObj = conf.MgmtObjects.Where(m => m.Type == MgmtObjectType.DynamicMemory).FirstOrDefault();
            Assert.IsNotNull(dynamicMemMgmtObj);
            Assert.AreEqual(MgmtObjectReturnValueType.Single, dynamicMemMgmtObj.ReturnValueType);
        }
    }
}
