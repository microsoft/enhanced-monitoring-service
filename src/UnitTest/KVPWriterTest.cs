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
using System.Collections.Generic;
using EnhancedMonitoring;
using System.Management;
using EnhancedMonitoring.Configuration;

namespace UnitTest
{
    [TestClass]
    public class KVPWriterTest
    {

        private const String testKey = "TestKVPWriterKey1";
        private const String testKey2 = "TestKVPWriterKey2";

        //[TestMethod]
        public void TestOverloadKVP()
        {
            var vm = WMIHelper.QueryFirstInstacne(@"root\virtualization\v2", "Msvm_ComputerSystem ", "enabledstate = 2 and caption = 'Virtual Machine'");
            Assert.IsNotNull(vm);

            var writer = KeyValuePairWriter.CreateInstance(new KvpConfiguration());
            var data = new Dictionary<String, Object>();

            var chars = new char[1000];
            for(int i = 0; i < chars.Length; i++)
            {
                chars[i] = 'a';
            }
            var val = new String(chars);
            for (int i = 0; i < 1000; i++ )
            {
                data.Clear();
                for (int j = 0; j < 10; j++)
                {
                    data.Add(String.Format("TestKVP_{0}", j), val);
                }

                var args = new Dictionary<String, Object>() { 
                    {"VirtualMachinePath", vm.Path.ToString()},
                    {"VirtualMachineName", vm.Properties["Name"].Value},
                };

                writer.Remove(args, data);
                writer.Write(args, data);
            }          
            vm.Dispose(); 
        }

        [TestMethod]
        public void TestWriteKVP()
        {

            var vm = WMIHelper.QueryFirstInstacne(@"root\virtualization\v2", "Msvm_ComputerSystem ", "enabledstate = 2 and caption = 'Virtual Machine'");
            Assert.IsNotNull(vm);

            var writer = KeyValuePairWriter.CreateInstance(new KvpConfiguration());
            var data = new Dictionary<String, Object>();
            data.Add(testKey, DateTime.Now.ToString());
            data.Add(testKey2, DateTime.Now.ToString());

            var args = new Dictionary<String, Object>() { 
                {"VirtualMachinePath", vm.Path.ToString()},
                {"VirtualMachineName", vm.Properties["Name"].Value},
            };

            writer.Remove(args, data);
            writer.Write(args, data);

            vm.Dispose();            
        }

        //Uncomment the following line to cleanup kvp
        //[TestMethod]
        public void CleanupKVP()
        {
            using (var vm = WMIHelper.QueryFirstInstacne(@"root\virtualization\v2", "Msvm_ComputerSystem ", "enabledstate = 2 and caption = 'Virtual Machine'"))
            {
                Assert.IsNotNull(vm);
                Console.WriteLine("Clean up kvp for: " + vm.Properties["ElementName"].Value);
                var writer = KeyValuePairWriter.CreateInstance(new KvpConfiguration());

                var NULL = new Object();

                var data = new Dictionary<String, Object>();
                data.Add(testKey, NULL);
                data.Add(testKey2, NULL); 

                var args = new Dictionary<String, Object>() 
                { 
                    {"VirtualMachinePath", vm.Path.ToString()},
                    {"VirtualMachineName", vm.Properties["Name"].Value},
                };

                writer.Remove(args, data);

                data.Clear();
                for (int i = 0; i < 50; i++)
                {
                    data.Add("Enhanced_Monitoring_Metric_Data_Item_Part_" + i, NULL);                    
                }
                writer.Remove(args, data);                
            }

        }
    }
}
