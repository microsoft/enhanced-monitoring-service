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
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EnhancedMonitoring
{
    public class SupportedVMDetector
    {
        protected const String WMI_Namespace = @"root\virtualization\v2";
        protected const String WMI_Class_Msvm_ComputerSystem = @"Msvm_ComputerSystem";
        protected const String WMI_Class_Msvm_KvpExchangeComponent = @"Msvm_KvpExchangeComponent";

        public static SupportedVMDetector CreateInstance(SupportedVMDetectorConfiguration conf)
        {
            return new SupportedVMDetector(conf);
        }

        private String SupportedVMKey;
        public SupportedVMDetector(SupportedVMDetectorConfiguration conf)
        {
            this.SupportedVMKey = conf.GuestDataItemKey;
        }

        public IList<IDictionary<String, Object>> GetSupportedVM()
        {
            using (var vms = WMIHelper.QueryInstacnes(WMI_Namespace, WMI_Class_Msvm_ComputerSystem, "EnabledState = 2 and Caption='Virtual Machine'"))
            {
                var tasks = new List<Task<IDictionary<String, Object>>>();
                foreach(var vm in vms.Cast<ManagementObject>())
                {
                    tasks.Add(Task.Run<IDictionary<String, Object>>(() => 
                    {
                        var objs = vm.GetRelated(WMI_Class_Msvm_KvpExchangeComponent);
                        foreach (var obj in objs)
                        {
                            var kvps = obj.Properties["GuestExchangeItems"].Value as String[];

                            if (kvps.Any(kvp => kvp.IndexOf(this.SupportedVMKey) >= 0))
                            {
                                var vmdata = new Dictionary<String, Object>();
                                vmdata.Add("VirtualMachineName", vm.Properties["Name"].Value);
                                vmdata.Add("VirtualMachineElementName", vm.Properties["ElementName"].Value);
                                vmdata.Add("VirtualMachinePath", vm.Path);
                                vmdata.Add("GuestExchangeItems", kvps);
                                return vmdata;
                            }
                        }
                        return null;
                    }));
                }
                try
                {
                    var task = Task.WhenAll<IDictionary<String, Object>>(tasks);
                    task.Wait();
                    return task.Result.Where(x => x != null).ToList();
                }
                catch(AggregateException e)
                {
                    foreach(var inner in e.InnerExceptions)
                    {
                        Logger.Error(inner);
                        throw inner;
                    }
                    throw;
                }
            }
        }

    }
}
