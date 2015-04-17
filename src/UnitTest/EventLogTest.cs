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
using System.Diagnostics;
using EnhancedMonitoring.Logging;
using System.Threading;
using EnhancedMonitoring.Configuration;

namespace UnitTest
{
    [TestClass]
    public class EventLogTest
    {
        public void TestCreateEventLogSource()
        {
            EventLog.CreateEventSource(new EventSourceCreationData(EventLogWriter.Source, EventLogWriter.LogName) 
            { 
                MachineName = System.Environment.MachineName
            });
        }

        public void TestRemoveEventLogSource()
        {
            EventLog.DeleteEventSource(EventLogWriter.Source);
        }

        //[TestMethod]
        public void TestEventLogSource()
        {
            Assert.IsTrue(EventLog.Exists(EventLogWriter.LogName));
            //Assert.IsTrue(EventLog.SourceExists(EventLogWriter.Source));

            using (EventLog log = new EventLog())
            {
                log.Log = EventLogWriter.LogName;
                log.Source = EventLogWriter.Source;
                log.MachineName = System.Environment.MachineName;
                log.WriteEntry("Test", EventLogEntryType.Warning);
            }
        }

        [TestMethod]
        public void TestEventLogFlushInterval()
        {
            var writer = new ShimEventLogWriter(new MonitorConfiguration() 
            { 
                EventLogInterval = 100.0 / 60000 //100ms
            });
                
            //Try to refresh timestamp for last flush.
            //A flush will be triggered if required.
            //And the timestamp will be reset to NOW
            writer.Write(LogLevel.Error, "");

            //Clear flushed flag and write a log record.
            writer.Flushed = false;
            writer.Write(LogLevel.Info, "");
            Assert.IsFalse(writer.Flushed);

            //Clear flushed flag and wait for 120ms
            writer.Flushed = false;
            Thread.Sleep(120);

            //Trigger a flush and check the flag.
            writer.Write(LogLevel.Info, "");
            Assert.IsTrue(writer.Flushed);
        }
    }

    class ShimEventLogWriter:EventLogWriter
    {

        public ShimEventLogWriter(MonitorConfiguration conf) : base(conf) { }

        public bool Flushed { get; set; }
        protected override void WriteEventLog()
        {
            this.Flushed = true;
        }
    }
}
