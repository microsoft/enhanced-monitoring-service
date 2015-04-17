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
﻿using EnhancedMonitoring.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMonitoring.Logging
{
    public class Logger
    {
        private static Object instanceLock = new Object();
        private static Logger instance = null;
        private const String DateFormat = "yyyy-MM-dd hh:mm:ss.fff";
        public static Logger Instance
        {
            get
            {
                lock(instanceLock)
                {
                    if (instance == null)
                    {
                        throw new NullReferenceException("Log hasn't been initialized");
                    }
                }
                return instance;
            }
        }

        public static void Init(MonitorConfiguration conf)
        {
            Init(conf, new LogWriter[] 
            {
                 new FileLogWriter(conf),
                 new EventLogWriter(conf)
            });
        }

        public static void Init(MonitorConfiguration conf, IList<LogWriter> writers)
        {
            lock (instanceLock)
            {
                if (instance == null)
                {
                    instance = new Logger(conf);
                    instance.AddWriters(writers);
                }
            }
        }
        
        public static void Verbose(String format, params Object[] args)
        {
            Logger.Instance.Log(LogLevel.Verbose, format, args);
        }

        public static void Verbose(Exception e)
        {
            Verbose(ExceptionToString(e));
        }

        public static void Info(String format, params Object[] args)
        {
            Logger.Instance.Log(LogLevel.Info, format, args);
        }

        public static void Info(Exception e)
        {
            Info(ExceptionToString(e));
        }

        public static void Warn(String format, params Object[] args)
        {
            Logger.Instance.Log(LogLevel.Warning, format, args);
        }

        public static void Warn(Exception e)
        {
            Warn(ExceptionToString(e));
        }
        
        public static void Error(String format, params Object[] args)
        {
            Logger.Instance.Log(LogLevel.Error, format, args);
        }

        public static void Error(Exception e)
        {
            Error(ExceptionToString(e));
        }

        private static String ExceptionToString(Exception e)
        {
            return String.Format("{0}\n{1}", e, e.StackTrace);
        }

        private LogLevel logLevel = LogLevel.Info;
        private List<LogWriter> writers = new List<LogWriter>();

        private Logger(MonitorConfiguration conf)
        {
            Enum.TryParse<LogLevel>(conf.LogLevel, out this.logLevel);
        }

        public void AddWriters(IList<LogWriter> writers)
        {
            this.writers.AddRange(writers);
        }

        public void Log(LogLevel level, String format, params Object[] args)
        {
            if(level < logLevel)
            {
                return;
            }

            String logMsg = format;
            if(args != null && args.Count() != 0)
            {
                logMsg = String.Format(format, args);
            }
             
            String date = DateTime.Now.ToString(DateFormat);
            String msg = String.Format("{0} {1}\t{2}", date, level.ToString().ToUpper(), logMsg);
            foreach(var writer in writers)
            {
                writer.Write(level, msg);
            }
        }               
    }
}
