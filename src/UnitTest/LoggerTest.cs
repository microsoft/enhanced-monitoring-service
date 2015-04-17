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
﻿using EnhancedMonitoring.Logging;
using EnhancedMonitoring.Configuration;
using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// Summary description for LoggerTest
    /// </summary>
    [TestClass]
    public class LoggerTest
    {
        [TestMethod]
        public void TestLog()
        {
            var conf = new MonitorConfiguration()
            {
                LogLevel = "Info"
            };
            var logWriter = new TestLogWriter();
            Logger.Init(conf, new LogWriter[] { logWriter });
            
            Logger.Verbose("Trival");
            Assert.IsNull(logWriter.Msg);

            Logger.Info("Formated information: {0}", "Nothing");
            Assert.IsTrue(logWriter.Msg.Contains("Formated information"));
            Assert.AreEqual(LogLevel.Info, logWriter.Level);
            Console.WriteLine(logWriter.Msg);

            try
            {
                RaiseException();
            }
            catch(Exception e)
            {
                Logger.Error(e);
                //Check call stack is logged.
                Assert.IsTrue(logWriter.Msg.Contains("at UnitTest.LoggerTest.TestLog()"));
                Console.WriteLine(logWriter.Msg);
            }
        }

        [TestMethod]
        public void TestLogLevelConfigure()
        {
            var conf = new MonitorConfiguration()
            {
                LogLevel = "Infoasdfasdf"
            };
            var logWriter = new TestLogWriter();
            Logger.Init(conf, new LogWriter[] { logWriter });
            Logger.Info("Test");
            Assert.AreEqual(LogLevel.Info, logWriter.Level);
        }

        [TestMethod]
        public void TestZeroLogWriter()
        {
            var conf = new MonitorConfiguration()
            {
                LogLevel = "Infoasdfasdf"
            };
     
            Logger.Init(conf, new LogWriter[0]);
            Logger.Info("Test");
        }

        private void RaiseException()
        {
            throw new NotImplementedException();
        }
    }

    class TestLogWriter : LogWriter
    {
        public LogLevel Level { get; set; }
        public String Msg { get; set; }

        public void Write(LogLevel level, String msg)
        {
            this.Level = level;
            this.Msg = msg;
        }
    }
}
