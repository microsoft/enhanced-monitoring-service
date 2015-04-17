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
using EnhancedMonitoring.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnhancedMonitoring.Service
{
    public partial class EnhacedMonitoring : ServiceBase
    {
        private const Int64 MIN_REFRESH_RATE = 60; //Default minimal refresh rate is 60 seconds

        public EnhacedMonitoring()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var configFilePath = String.Format(@"{0}\{1}\{2}",
                            System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                            "Enhanced Monitoring", "EnhancedMonitoringProviderConfig.xml");

            var defaultLogPath = String.Format(@"{0}\{1}\{2}\{3}",
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                        "Enhanced Monitoring", "log", "monitor.log");

            MonitorConfiguration conf = MonitorConfiguration.Load(configFilePath);
            conf.LogFilePath = conf.LogFilePath ?? defaultLogPath;
            Logger.Init(conf);
            timer = new Timer(_ => this.RunMonitoringTask(conf), null, 0, Timeout.Infinite);

            Logger.Info("Monitoring service started");
        }

        private Timer timer;

        private void RunMonitoringTask(MonitorConfiguration conf)
        {
            Logger.Info("Start monitoring task");
            DateTime start = DateTime.Now;
            MonitoringTask task = MonitoringTask.CreateInstance(conf);
            task.Run();
            Logger.Info("Monitoring task finished");
            TimeSpan elapsed = DateTime.Now.Subtract(start);
            //Substract elapsed time to get the task running exaclty aliging to refresh rate.
            long interval = Math.Max(conf.RefreshRate, MIN_REFRESH_RATE) * 1000;
            long timeToWait = interval - (long)elapsed.TotalMilliseconds % interval;
            //Trigger next task
            timer.Change(timeToWait, Timeout.Infinite);
        }

        protected override void OnStop()
        {
            if(this.timer != null)
            {
                this.timer.Dispose();
            }
        }
    }
}
