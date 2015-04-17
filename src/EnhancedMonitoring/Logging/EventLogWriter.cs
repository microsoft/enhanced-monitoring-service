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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMonitoring.Logging
{
    public class EventLogWriter : LogWriter
    {
        public const String LogName = "Enhanced Monitoring";
        public const String Source = "Enhanced Monitoring Provider Service";
        public const String EventLogMsgFormat = "There are {0} error(s), {1} warning(s) during the last {2} minute(s). " 
            + "For more information please view the log file, \"{3}\".";
        public const double DefaultLogInterval = 60;

        private int warningCount;
        private int errorCount;
        private DateTime lastFlush = DateTime.Now;
        private double flushIntervalInMinutes;
        private String logFilePath;

        public EventLogWriter(Configuration.MonitorConfiguration conf)
        {
            this.logFilePath = conf.LogFilePath;
            this.flushIntervalInMinutes = conf.EventLogInterval <= 0 ? DefaultLogInterval : conf.EventLogInterval;
        }

        public void Write(LogLevel level, String msg)
        {
            switch(level)
            {
                case LogLevel.Warning:
                    warningCount++;
                    break;
                case LogLevel.Error:
                    errorCount++;
                    break;
                default:
                    break;
            }
            if(errorCount > 0 || warningCount > 0)
            {
                DateTime now = DateTime.Now;
                if (now.Subtract(this.lastFlush).TotalMinutes > this.flushIntervalInMinutes)
                {
                    WriteEventLog();
                    this.lastFlush = now;
                    errorCount = 0;
                    warningCount = 0;
                }
            }
        }

        protected virtual void WriteEventLog()
        {
            String msg = String.Format(EventLogMsgFormat, errorCount, warningCount, 
                                       this.flushIntervalInMinutes, this.logFilePath);
            using (EventLog log = new EventLog(EventLogWriter.LogName))
            {
                log.Source = EventLogWriter.Source;
                log.WriteEntry(msg, this.errorCount > 0 ? EventLogEntryType.Error : EventLogEntryType.Warning);
            }
        }        
    }
}
