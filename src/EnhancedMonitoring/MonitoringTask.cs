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
using EnhancedMonitoring.DataFormat;
using EnhancedMonitoring.DataCollector;
using EnhancedMonitoring.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMonitoring
{
    public class MonitoringTask
    {
        public const String VersionKey = "Version";

        public static MonitoringTask CreateInstance(MonitorConfiguration conf)
        {
            if (conf == null)
            {
                throw new ArgumentNullException("conf");
            }

            var instance = new MonitoringTask(conf);
            return instance;
        }

        protected List<MgmtObject> mgmtObjects = new List<MgmtObject>();
        protected MonitorConfiguration conf;

        protected MonitoringTask(MonitorConfiguration conf)
        {
            this.conf = conf;
            foreach (var mgmtObjConf in conf.MgmtObjects)
            {
                mgmtObjects.Add(MgmtObject.CreateInstance(mgmtObjConf));
            }
        }

        public void Run()
        {
            var vms = GetSupportedVM();
            if (vms == null || vms.Count == 0)
            {
                Logger.Info("No supported VM found.");
                return;
            }

            Logger.Info("Detected {0} supported VMs", vms.Count);

            List<Task> tasks = new List<Task>();
            foreach (var vm in vms)
            {
                tasks.Add(Task.Run(() =>
                {
                    var data = CollectDataForVM(vm);
                    var packedData = new XmlDataFormatter(this.conf).ToXml(data);
                    WriteKVPToVM(vm, packedData);
                }));
            }
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException e)
            {
                foreach (var inner in e.InnerExceptions)
                {
                    Logger.Error(inner);
                    throw inner;
                }
            }
        }

        private void WriteKVPToVM(IDictionary<string, object> vm, IDictionary<string, object> packedData)
        {

            Logger.Info(String.Format("Write {0} kvps to VM {1}", packedData.Count, vm["VirtualMachineElementName"]));
            using (KeyValuePairWriter writer = KeyValuePairWriter.CreateInstance(this.conf.Kvp))
            {
                try
                {
                    writer.Remove(vm, packedData);
                    writer.Write(vm, packedData);
                }
                catch (InvalidOperationException e)
                {
                    Logger.Info("Couldn't write KVP to VM {0}. The VM has been shutdown or removed.", 
                                vm["VirtualMachineElementName"]);
                    Logger.Verbose(e);
                }
                catch (ManagementException e)
                {
                    Logger.Error(e);
                }
                catch (NotImplementedException e)
                {
                    Logger.Info("Couldn't write KVP to VM {0}. The VM has been shutdown or removed.",
                                vm["VirtualMachineElementName"]);
                    Logger.Verbose(e);
                }
            }
        }

        private Dictionary<string, object> CollectDataForVM(IDictionary<string, object> vm)
        {
            var data = new Dictionary<String, Object>();

            //Add pre-defined argument, VirtualMachine
            var args = new Dictionary<String, Object>(vm);

            foreach (var mgmtObj in this.mgmtObjects)
            {

                data.Add(mgmtObj.KeyName, null);
                try
                {
                    data[mgmtObj.KeyName] = mgmtObj.CollectData(args);
                }
                catch (InvalidOperationException e)
                {
                    if (!mgmtObj.SuppressError)
                    {
                        Logger.Verbose(e);
                    }
                }
                catch (ManagementException e)
                {
                    if (!mgmtObj.SuppressError)
                    {
                        Logger.Error(e);
                    }
                }
                catch (ArgumentException e)
                {
                    if (!mgmtObj.SuppressError)
                    {
                        Logger.Error(e);
                    }
                }
                catch (NotImplementedException e)
                {
                    if (!mgmtObj.SuppressError)
                    {
                        Logger.Verbose(e);
                    }
                }
            }

            data.Add(VersionKey, this.conf.Version);
            return data;
        }

        private IList<IDictionary<String, Object>> GetSupportedVM()
        {

            IList<IDictionary<String, Object>> vms = null;
            try
            {
                vms = SupportedVMDetector.CreateInstance(this.conf.SupportedVMDetector).GetSupportedVM();
            }
            catch (InvalidOperationException e)
            {
                Logger.Verbose(e);
            }
            catch (ManagementException e)
            {
                Logger.Error(e);
            }
            catch (NotImplementedException e)
            {
                Logger.Verbose(e);
            }
            return vms;
        }

    }
}
