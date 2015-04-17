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
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMonitoring
{
    public class KeyValuePairWriter : IDisposable
    {
        protected const String WMI_Namespace = @"root\virtualization\v2";
        protected const String WMI_Class_Msvm_ComputerSystem = @"Msvm_ComputerSystem";
        protected const String WMI_Class_Msvm_VirtualSystemManagementService = @"Msvm_VirtualSystemManagementService";
        protected const String WMI_Class_Msvm_KvpExchangeDataItem = @"Msvm_KvpExchangeDataItem";
        protected const Int32 DEFAULT_WRITE_INTERVAL = 100;

        public static KeyValuePairWriter CreateInstance(KvpConfiguration conf)
        {
            return new KeyValuePairWriter(conf);
        }

        private bool batchMode = false;
        private Int32 writeInterval = DEFAULT_WRITE_INTERVAL;

        protected KeyValuePairWriter(KvpConfiguration conf)
        {
            if(conf != null)
            {
                this.batchMode = conf.BatchMode;
                this.writeInterval = conf.WriteInterval <= 0 ? DEFAULT_WRITE_INTERVAL : conf.WriteInterval;
            }
        }

        public void Remove(IDictionary<String, Object> args, IDictionary<String, Object> data)
        {  
            InvokeKvpOperation(args, data, "RemoveKvpItems");
        }

        public void Write(IDictionary<String, Object> args, IDictionary<String, Object> data)
        {
            InvokeKvpOperation(args, data, "AddKvpItems");                
        }

        protected void InvokeKvpOperation(IDictionary<String, Object> args, IDictionary<String, Object> data, String operationName)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Count() == 0)
            {
                return;
            }

            var kvpItems = ToKvpItems(data);
            using (var vmMgmt = WMIHelper.GetFirstInstance(WMI_Namespace, WMI_Class_Msvm_VirtualSystemManagementService))
            using (var vm = WMIHelper.QueryFirstInstacne(WMI_Namespace, WMI_Class_Msvm_ComputerSystem, String.Format("Name='{0}'", args["VirtualMachineName"])))
            {
                if(batchMode)
                {
                    InvokeKvpOperation(vmMgmt, vm, operationName, kvpItems);
                }
                else
                {
                    foreach (var kvpItem in kvpItems)
                    {
                        InvokeKvpOperation(vmMgmt, vm, operationName, new String[] { kvpItem });
                    }
                }
                 
            }
        }

        protected void InvokeKvpOperation(ManagementObject vmMgmt, ManagementObject vm, String operationName, String[] kvpItems)
        {
            using (var inParams = vmMgmt.GetMethodParameters(operationName))
            {
                inParams["DataItems"] = kvpItems;
                inParams["TargetSystem"] = vm;

                using (var outParams = vmMgmt.InvokeMethod(operationName, inParams, null))
                {
                    //Delay a small mount of time to avoid overwhelm the KVP VSC in the guest.
                    Task.Delay(this.writeInterval);
                    WMIHelper.WaitForAsyncJob(outParams, WMI_Namespace);
                }
            }   
        } 

        protected static String ToKvpItem(KeyValuePair<String, Object> kvp)
        {
            using (var kvpItem = WMIHelper.CreateInstance(WMI_Namespace, WMI_Class_Msvm_KvpExchangeDataItem))
            {
                kvpItem["Name"] = kvp.Key;
                kvpItem["Data"] = kvp.Value;
                kvpItem["Source"] = 0;
                return ((ManagementBaseObject)kvpItem).GetText(TextFormat.CimDtd20);
            }
        }

        protected static String[] ToKvpItems(IDictionary<String, Object> data)
        {
            var dataItems = data.Select((kvp) =>
            {
                return ToKvpItem(kvp);
            }).ToArray();
            return dataItems;
        }

        #region IDisposable
        bool disposed = false;

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {

            }

            disposed = true;
        }
        #endregion

    }
}
