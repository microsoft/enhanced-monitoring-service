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
using EnhancedMonitoring.Configuration;
using EnhancedMonitoring.DataCollector;
using EnhancedMonitoring;
using System.Collections.Generic;
using System.Management;
using System.Linq;

namespace UnitTest
{
    [TestClass]
    public class MgmtObjectTest
    {
        [TestMethod]
        public void TestMgmtObjectSelectAll()
        {
            MgmtObjectConfiguration conf = new MgmtObjectConfiguration()
            {
                Namespace = @"root\cimv2",
                From = @"WIN32_PerfRawData_HvStats_HyperVHypervisorLogicalProcessor",
                Where = @"Name='_Total'",
                PerfCounters = new PerfCounterConfigurationList()
            };

            conf.PerfCounters.Add(new PerfCounterConfiguration()
            {
                Select = "*"
            });

            var mgmtObj = MgmtObject.CreateInstance(conf);
            var data = mgmtObj.CollectData(new Dictionary<String, Object>()) as IList<Object>;
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestMgmtObjectSelectPath()
        {
            MgmtObjectConfiguration conf = new MgmtObjectConfiguration()
            {
                Namespace = @"root\cimv2",
                From = @"WIN32_PerfRawData_HvStats_HyperVHypervisorLogicalProcessor",
                Where = @"Name='_Total'",
                PerfCounters = new PerfCounterConfigurationList()
            };

            conf.PerfCounters.Add(new PerfCounterConfiguration()
            {
                Select = "*"
            });

            var mgmtObj = MgmtObject.CreateInstance(conf);
            var data = mgmtObj.CollectData(new Dictionary<String, Object>()) as IList<Object>;
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestMgmtObjectCollectData()
        {
            MgmtObjectConfiguration conf = new MgmtObjectConfiguration()
            {
                Namespace = @"root\cimv2",
                From = @"WIN32_PerfRawData_HvStats_HyperVHypervisorLogicalProcessor",
                Where = @"Name='_Total'",
                PerfCounters = new PerfCounterConfigurationList()
            };

            conf.PerfCounters.Add(new PerfCounterConfiguration() {
                As = "PercentHypervisorRuntime",
                Select = "PercentHypervisorRuntime"
            });
            
            var mgmtObj = MgmtObject.CreateInstance(conf);
            var data = mgmtObj.CollectData(new Dictionary<String,Object>());
            var list = (List<Object>) data;
            Assert.IsNotNull(list);

            var jsonObj = (IDictionary<String, Object>) list.FirstOrDefault();
            Assert.IsNotNull(jsonObj["PercentHypervisorRuntime"]);
            //Prevent PropertyData object is returned
            //The value in PropertyData should be returned not the container
            Assert.IsFalse(jsonObj["PercentHypervisorRuntime"] is PropertyData);
        }

        //[TestMethod]
        public void TestMgmtObjectUnicode()
        {
            MgmtObjectConfiguration conf = new MgmtObjectConfiguration()
            {
                Namespace = @"root\cimv2",
                From = @"Win32_PerfRawData_HvStats_HyperVHypervisorVirtualProcessor",
                Where = (@"Name like '%%虚拟机%%'"),
                PerfCounters = new PerfCounterConfigurationList()
            };

            conf.PerfCounters.Add(new PerfCounterConfiguration()
            {
                As = "PercentHypervisorRuntime",
                Select = "PercentHypervisorRuntime"
            });

            var mgmtObj = MgmtObject.CreateInstance(conf);
            var data = mgmtObj.CollectData(new Dictionary<String, Object>());
            var list = (List<Object>)data;
            Assert.IsNotNull(list);

            var jsonObj = (IDictionary<String, Object>)list.FirstOrDefault();
            Assert.IsNotNull(jsonObj["PercentHypervisorRuntime"]);
            //Prevent PropertyData object is returned
            //The value in PropertyData should be returned not the container
            Assert.IsFalse(jsonObj["PercentHypervisorRuntime"] is PropertyData);
        }

        [TestMethod]
        public void TestMgmtObjectErrorHandling()
        {
            //Test namespace wrong
            try
            {
                MgmtObjectConfiguration conf = new MgmtObjectConfiguration()
                {
                    Namespace = @"root\FALSE_NAMESPACE",
                    From = "WIN32_PerfRawData_HvStats_HyperVHypervisorLogicalProcessor",
                    PerfCounters = new PerfCounterConfigurationList() {
                        new PerfCounterConfiguration()
                    }
                };

                var mgmtObj = MgmtObject.CreateInstance(conf);
                var data = mgmtObj.CollectData(new Dictionary<String, Object>());
                Assert.Fail("ManagementException expected");
            }
            catch(ManagementException e)
            {
                Console.WriteLine(e);
                Console.WriteLine();
            }

            //Test class name wrong
            try
            {
                MgmtObjectConfiguration conf = new MgmtObjectConfiguration()
                {
                    Namespace = @"root\cimv2",
                    From = @"FALSE_CLASS",
                    PerfCounters = new PerfCounterConfigurationList(){
                        new PerfCounterConfiguration()
                    }
                }; 
                var mgmtObj = MgmtObject.CreateInstance(conf);
                var data = mgmtObj.CollectData(new Dictionary<String, Object>());
                Assert.Fail("ManagementException expected");
            }
            catch (ManagementException e)
            {
                Console.WriteLine(e);
                Console.WriteLine();
            }

            //Test no instance found
            try
            {
                MgmtObjectConfiguration conf = new MgmtObjectConfiguration()
                {
                    Namespace = @"root\cimv2",
                    From = @"WIN32_PerfRawData_HvStats_HyperVHypervisorLogicalProcessor",
                    Where = @"Name='FALSE_NAME'",
                    PerfCounters = new PerfCounterConfigurationList(){
                        new PerfCounterConfiguration()
                    }
                };

                var mgmtObj = MgmtObject.CreateInstance(conf);
                var data = mgmtObj.CollectData(new Dictionary<String, Object>());
                Assert.Fail("ManagementException expected");
            }
            catch (NotImplementedException e)
            {
                Console.WriteLine(e); 
                Console.WriteLine();
            }

            //Test null perf count            
            {
                MgmtObjectConfiguration conf = new MgmtObjectConfiguration()
                {
                    Namespace = @"root\cimv2",
                    From = @"WIN32_PerfRawData_HvStats_HyperVHypervisorLogicalProcessor",
                    Where = @"Name='_Total'",
                };

                var mgmtObj = MgmtObject.CreateInstance(conf);
                var data = mgmtObj.CollectData(new Dictionary<String, Object>());
                Assert.IsNull(data);
            }
        }
    }
}
