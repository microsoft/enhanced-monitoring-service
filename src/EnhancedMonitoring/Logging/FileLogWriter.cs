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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnhancedMonitoring.Logging
{
    public class FileLogWriter : LogWriter
    {
        public const Int64 DefaultLogFileSize = 4 * 1024 * 1024;
        public const Int32 DefaultLogRetention = 10;

        private String filePath;
        private Int64 logFileSize;
        private Int32 maxLogRetention;
        private Object writeLock = new Object();

        public FileLogWriter(Configuration.MonitorConfiguration conf)
        {
            this.filePath = conf.LogFilePath;
            this.logFileSize = conf.LogFileSize <= 0 ? DefaultLogFileSize : conf.LogFileSize;
            this.maxLogRetention = conf.MaxLogRetention <= 0 ? DefaultLogRetention : conf.MaxLogRetention;
            Directory.CreateDirectory(Path.GetDirectoryName(this.filePath));
        }

        public void Write(LogLevel level, String msg)
        {
            lock(writeLock)
            {
                FileInfo file = new FileInfo(this.filePath);
                if (file.Exists && file.Length > this.logFileSize)
                {
                    RotateLogFile();
                }
                File.AppendAllLines(this.filePath, new String[] { msg }, Encoding.UTF8);
            }                        
        }

        /// <summary>
        /// 
        /// Rotate the old log to another file. The rule is:
        /// 1. log -> log.0
        /// 2. log.$(i) -> log.$(i + 1)
        /// 3. IF i + 1 == LogRetention: delete log.$(i)
        /// 
        /// </summary>
        protected void RotateLogFile()
        {
            int index = 0;
            String rotateFilePath = GetRotateFilePath(index);
            while(File.Exists(rotateFilePath))
            {
                //Reaches retention limit, delete the oldest log
                if (index == this.maxLogRetention - 1)
                {
                    File.Delete(rotateFilePath);
                    break;
                }
                index++;
                rotateFilePath = GetRotateFilePath(index);          
            }

            for(int i = index; i > 0; i--)
            {
                String src = GetRotateFilePath(i - 1);
                String dest = GetRotateFilePath(i);
                File.Move(src, dest);
            }
            rotateFilePath = String.Format("{0}.{1}", this.filePath, 0);
            File.Move(this.filePath, rotateFilePath);
        }

        private String GetRotateFilePath(int index)
        {
            return String.Format("{0}.{1}", this.filePath, index);
        }
    }
}
