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
using EnhancedMonitoring.DataFormat;
using EnhancedMonitoring.DataCollector;
using EnhancedMonitoring;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class MetricTest
    {
        [TestMethod]
        public void TestMetricJSON()
        {
            MonitorConfiguration conf = MonitorConfiguration.Load(@"..\..\..\EnhancedMonitoring\Configuration\EnhancedMonitoringProviderConfig.xml");
            List<MgmtObject> mgmtObjects = new List<MgmtObject>();
            foreach (var mgmtObjConf in conf.MgmtObjects)
            {
                mgmtObjects.Add(MgmtObject.CreateInstance(mgmtObjConf));
            }

            Assert.AreNotEqual(0, mgmtObjects.Count);


            var data = new Dictionary<String, Object>();
            var vms = SupportedVMDetector.CreateInstance(conf.SupportedVMDetector).GetSupportedVM();
            Assert.AreNotEqual(0, vms.Count);

            var vm = vms.FirstOrDefault();

            var args = new Dictionary<String, Object>(vm);

            foreach (var mgmtObj in mgmtObjects)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    data.Add(mgmtObj.KeyName, mgmtObj.CollectData(args));
                }
                catch (Exception e)
                {
                    if (!mgmtObj.SuppressError)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine(e.StackTrace);
                        Assert.Fail(e.Message);
                    }                    
                    data.Add(mgmtObj.KeyName, null);
                }
                Console.WriteLine(String.Format("{0}\t{1}", watch.ElapsedMilliseconds, mgmtObj.KeyName));
            }

            Assert.IsNotNull(data);
            Assert.AreNotEqual(0, data.Count);

            var xmlStr = DataFormatHelper.ToXml(data);
            Console.WriteLine(xmlStr);

            //var jsonStr = DataFormatHelper.ToJSON(data);

            //var encodedStr = DataFormatHelper.Base64Encode(jsonStr);

            //var dataChunks = DataFormatHelper.PackString(jsonStr, 900);

            //var packedData = DataFormatHelper.ToChunk("", dataChunks);

            //var encodedDataChunks = DataFormatHelper.PackString(encodedStr, 900);

            //var encodedPackedData = DataFormatHelper.ToChunk("", encodedDataChunks);

            //Console.WriteLine(JsonHelper.FormatJson(jsonStr));
            //Console.WriteLine(String.Format("Original   JSON      string length: {0}", jsonStr.Length));
            //Console.WriteLine(String.Format("Encoded    JSON      string length: {0}", encodedStr.Length));
            //Console.WriteLine(String.Format("Packed     JSON      string length: {0}", packedData.Select(d => d.Value.ToString().Length).Sum()));
            //Console.WriteLine(String.Format("Packed encoded  JSON string length: {0}", encodedPackedData.Select(d => d.Value.ToString().Length).Sum()));

        }

        //[TestMethod]
        public void TestWMIPerformanceCache()
        {
            Console.WriteLine("First time query");
            TestMetricJSON();
            System.Threading.Thread.Sleep(60 * 1000); 
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine("Second time query, after 60seconds");
            TestMetricJSON();
        }

        //[TestMethod]
        public void TestWMIPerformanceMultiple()
        {
            for(int i= 0; i < 10; i++)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                TestMetricJSON();
                Console.WriteLine(String.Format("Elapsed: {0}ms \t Total", watch.ElapsedMilliseconds));
                Console.WriteLine("----------------------------------------------------------------------");
            }
        }
    }
}
