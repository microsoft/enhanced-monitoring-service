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
using EnhancedMonitoring.DataFormat;
using System.Runtime.Serialization;

namespace UnitTest
{
    [TestClass]
    public class FormatHelperTest
    {
        [TestMethod]
        public void TestToXml()
        {
            String expected = System.IO.File.ReadAllText(@"..\..\TestToXmlExpectedOutput.xml");
            var data = new Dictionary<String, Object>();
            data.Add("Name", "Dictionary(JSON object) to Xml Test");
            data.Add("TestCases", new String[] 
            { 
                "Case1: String as Field", 
                "Case2: String Array as Field", 
                "Case3: Dictionary as Field", 
                "Case4: Object Array as Field",
                "Case5: Null"
            });
            data.Add("ExpectedBehavior", new Dictionary<String, Object>() { 
                {"ForAll", "Key name as XElement name "},
                {"Case1", "String content as XElement content"}, 
                {"Case2", "Each element becomes a child XElement with name 'Item'"}, 
                {"Case3", "Dictionary becomes a child XElement"},
                {"Case4", new String[]
                    {
                        "1. Each element in the array is a dictionary", 
                        "2. The 1st dictionary itself contains an array field",
                        "3. The 2nd dictionary itself contains a dictionary"
                    }
                },
                {"Case5", "Output 'null'"}
            });
            data.Add("ObjectArray", new Dictionary<String, Object>[] { 
                new Dictionary<String, Object>()
                {
                    {"ArrayObject", new String[]{"Element 1", "Element 1"}}
                },
                new Dictionary<String, Object>()
                {
                    {
                        "DictionaryObject", new Dictionary<String, Object>()
                        {
                            {"ObjectField", "ObjectValue"}
                        }
                    }
                },
            });
            data.Add("HandleNull", null);

            var xmlStr = DataFormatHelper.ToXml(data);
            Console.WriteLine(xmlStr);
            Assert.AreEqual(expected, xmlStr);
        }

        [TestMethod]
        public void TestToXmlNegative()
        {
            var data = new Dictionary<String, Object>();
            data.Add("", null);
            try
            {
                DataFormatHelper.ToXml(data);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("Node name can't be null or empty.", e.Message);
            }

            data.Clear();
            data.Add("Node Name with Space", "Space will be replaced with _");
            var xmlStr = DataFormatHelper.ToXml(data);
            String expected =
@"<Data>
  <Node_Name_with_Space>Space will be replaced with _</Node_Name_with_Space>
</Data>";
            Assert.AreEqual(expected, xmlStr);
        }
    }

}
