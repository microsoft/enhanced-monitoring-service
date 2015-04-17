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
using EnhancedMonitoring;
using EnhancedMonitoring.Configuration;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class SupportedVMDetectorTest
    {
        /// <summary>
        /// This test requires the host has at least one virtual machine to pass.
        /// </summary>
        [TestMethod]
        public void TestDetectSupportedVM()
        {
            DetectSupportedVM();
        }

        public IDictionary<String, Object> DetectSupportedVM()
        {
            var detector = SupportedVMDetector.CreateInstance(new SupportedVMDetectorConfiguration() 
            {
                GuestDataItemKey = "Enhanced_Monitoring_Supported", 
            });

            var watcher = System.Diagnostics.Stopwatch.StartNew();
            var vms = detector.GetSupportedVM();
            Console.WriteLine(String.Format("Elapsed: {0}ms\t Detect supported VM", watcher.ElapsedMilliseconds));

            Assert.IsNotNull(vms);
            Assert.AreNotEqual(0, vms.Count);

            var vm = vms.FirstOrDefault();
            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm["VirtualMachineName"]);
            Assert.IsNotNull(vm["VirtualMachineElementName"]);
            Assert.IsNotNull(vm["VirtualMachinePath"]);
            Console.WriteLine("Found: " + vm["VirtualMachineElementName"]);
            return vm;
        }
    }
}
