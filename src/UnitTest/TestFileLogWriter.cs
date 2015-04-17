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
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnhancedMonitoring.Configuration;
using EnhancedMonitoring.Logging;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace UnitTest
{
    /// <summary>
    /// Summary description for TestFileLogWriter
    /// </summary>
    [TestClass]
    public class TestFileLogWriter
    {

        [TestMethod]
        public void TestWriteToLogFile()
        {
            var tmpFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) 
                                                             + @"\" + Guid.NewGuid().ToString();

            var conf = new MonitorConfiguration()
            {
                LogLevel = "Info",
                LogFilePath = tmpFolder + @"\test.log"
            };

            var logWriter = new FileLogWriter(conf);

            String logContent = Guid.NewGuid().ToString();
            logWriter.Write(LogLevel.Info, logContent);

            Assert.IsTrue(File.Exists(conf.LogFilePath));
            String log = File.ReadAllText(conf.LogFilePath);
            Assert.IsTrue(log.Contains(logContent));

            Directory.Delete(tmpFolder, true);
        }

        [TestMethod]
        public void TestMultithreadWriteToLogFile()
        {
            var tmpFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData)
                                                             + @"\" + Guid.NewGuid().ToString();

            var conf = new MonitorConfiguration()
            {
                LogLevel = "Info",
                LogFilePath = tmpFolder + @"\test.log"
            };

            var logWriter = new FileLogWriter(conf);

            Task[] tasks = new Task[100];
            for(int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    Console.WriteLine(Process.GetCurrentProcess().Threads.Count);
                    String logContent = Guid.NewGuid().ToString();
                    logWriter.Write(LogLevel.Info, logContent);

                });  
            }                     
            Task.WaitAll(tasks);
            Directory.Delete(tmpFolder, true);
        }

        [TestMethod]
        public void TestRotateLog()
        {
            var tmpFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData)
                                                             + @"\" + Guid.NewGuid().ToString();

            Console.WriteLine(tmpFolder);
            Directory.CreateDirectory(tmpFolder);
            var conf = new MonitorConfiguration()
            {
                LogLevel = "Info",
                LogFilePath = tmpFolder + @"\test.log",
                LogFileSize = 1,
                MaxLogRetention = 2
            };

            var logWriter = new FileLogWriter(conf);
            
            String[] logContent = new String[4]
            {
                "Test 0","Test 1","Test 2","Test 3",
            };

            //echo "Test 0" > test.log
            logWriter.Write(LogLevel.Info, logContent[0]);
            //mv test.log test.log.0
            //echo "Test 1" > test.log
            logWriter.Write(LogLevel.Info, logContent[1]);

            Assert.IsTrue(File.Exists(conf.LogFilePath));
            String log = File.ReadAllText(conf.LogFilePath);
            Assert.IsTrue(log.Contains(logContent[1]));

            Assert.IsTrue(File.Exists(conf.LogFilePath + @".0"));
            log = File.ReadAllText(conf.LogFilePath + @".0");
            Assert.IsTrue(log.Contains(logContent[0]));

            //mv test.log.0  test.log.1
            //mv test.log test.log.0
            //echo "Test 2" > test.log
            logWriter.Write(LogLevel.Info, logContent[2]);

            //rm test.log.1
            //mv test.log.0 test.log.1
            //mv test.log test.log.0
            //echo "Test 3" test.log
            logWriter.Write(LogLevel.Info, logContent[3]);

            Assert.IsTrue(File.Exists(conf.LogFilePath + @".0"));
            Assert.IsTrue(File.Exists(conf.LogFilePath + @".1"));
            Assert.IsFalse(File.Exists(conf.LogFilePath + @".2")); 
            
            log = File.ReadAllText(conf.LogFilePath);
            Assert.IsTrue(log.Contains(logContent[3]));


            log = File.ReadAllText(conf.LogFilePath + @".1");
            Assert.IsTrue(log.Contains(logContent[1]));

            Directory.Delete(tmpFolder, true);
        }

    }
}
