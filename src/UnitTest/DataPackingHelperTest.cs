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
using EnhancedMonitoring.DataFormat;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class DataPackingHelperTest
    {
        [TestMethod]
        public void TestPackString()
        {
            {
                var rawStr = "aaaa";
                var result = DataFormatHelper.PackString(rawStr, 1);
                Assert.IsNotNull(result);
                Assert.AreEqual(4, result.Count);
            }

            {
                var rawStr = "aaaa";
                var result = DataFormatHelper.PackString(rawStr, 3);
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count);
            }

            {
                var rawStr = "aaaa";
                var result = DataFormatHelper.PackString(rawStr, 5);
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);
            }
        }

        [TestMethod]
        public void TestPackData()
        {
            var data = new Dictionary<String, Object>() 
            { 
                {"MaxClockFrequency", 100000},
                {"AvailableMemeory", 1024},
            };
            foreach (var kvp in data)
            {
                Console.WriteLine(kvp);
            }
            var dataStr = DataFormatHelper.ToXml(data);
            Console.WriteLine(dataStr);

            dataStr = DataFormatHelper.Base64Encode(dataStr);
            Console.WriteLine(dataStr);

            var dataChunks = DataFormatHelper.PackString(dataStr, 50);

            var packedData = DataFormatHelper.ToChunk("Part_", dataChunks, DataFormatHelper.ToXml);
            foreach(var kvp in packedData)
            {
                Console.WriteLine(kvp);
            }

            Assert.AreEqual(3, packedData.Count);
        }
    }
}
