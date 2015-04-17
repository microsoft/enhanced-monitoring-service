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

namespace EnhancedMonitoring.DataCollector
{
    public class DynamicMemoryMgmtObject : MgmtObject
    {
        public const String WMI_Namespace = @"root\virtualization\v2";
        public const String WMI_Class_Msvm_ComputerSystem = @"Msvm_ComputerSystem";
        public const String WMI_Class_Msvm_VirtualSystemManagementService = @"Msvm_VirtualSystemManagementService";
        public const String WMI_Class_Msvm_VirtualSystemSettingData = @"Msvm_VirtualSystemSettingData";
        public const String WMI_Method_GetSummaryInformation = @"GetSummaryInformation";

        public const UInt32 RequestedInformation_MemoryUsage = 103;
        public const String KeyName_MemoryUsage = "MemoryUsage";
        public const UInt32 RequestedInformation_MemoryAvailable = 112;
        public const String KeyName_MemoryAvailable = "MemoryAvailable";
        public const UInt32 RequestedInformation_AvaiableMemoryBuffer = 113;
        public const String KeyName_AvailableMemoryBuffer = "AvailableMemoryBuffer";
               

        public DynamicMemoryMgmtObject(MgmtObjectConfiguration conf) : base(conf)
        {
        }

        public override Object CollectData(IDictionary<String, Object> args)
        {
            using (var vmMgmt = WMIHelper.GetFirstInstance(WMI_Namespace, WMI_Class_Msvm_VirtualSystemManagementService))
            using (var vm = WMIHelper.QueryFirstInstacne(WMI_Namespace, WMI_Class_Msvm_ComputerSystem, 
                    String.Format("Name='{0}'", args["VirtualMachineName"])))
            using (var vmSettings = vm.GetRelated(WMI_Class_Msvm_VirtualSystemSettingData))
            using (var inParams = vmMgmt.GetMethodParameters(WMI_Method_GetSummaryInformation))
            {
                String[] settingDataPath = new String[vmSettings.Count];
                int i = 0;
                foreach (ManagementObject vmSetting in vmSettings)
                {
                    settingDataPath[i++] = vmSetting.Path.Path;
                    break;
                }
                var data = new Dictionary<String, Object>() 
                {
                    {KeyName_MemoryUsage, null},
                    {KeyName_MemoryAvailable, null},
                    {KeyName_AvailableMemoryBuffer, null},
                };

                if (settingDataPath.Length != 0)
                {
                    inParams["SettingData"] = settingDataPath;
                    inParams["RequestedInformation"] = new UInt32[] 
                    {
                        RequestedInformation_MemoryUsage, 
                        RequestedInformation_MemoryAvailable, 
                        RequestedInformation_AvaiableMemoryBuffer
                    };

                    var outParams = vmMgmt.InvokeMethod(WMI_Method_GetSummaryInformation, inParams, null);
                    if ((UInt32)outParams["ReturnValue"] == 0)//Completed
                    {
                        var summaryInfoList = (ManagementBaseObject[])outParams["SummaryInformation"];
                        var summaryInfo = summaryInfoList.FirstOrDefault();
                        data[KeyName_MemoryUsage] = summaryInfo[KeyName_MemoryUsage];
                        data[KeyName_MemoryAvailable] = summaryInfo[KeyName_MemoryAvailable];
                        data[KeyName_AvailableMemoryBuffer] = summaryInfo[KeyName_AvailableMemoryBuffer];
                    }
                    else
                    {
                        throw new ManagementException(String.Format("Method {0} returns error:{1}", WMI_Method_GetSummaryInformation,
                                (UInt32)outParams["ReturnValue"]));
                    }
                }
                return data;
            }
        }

        public override String KeyName
        {
            get
            {
                return this.conf.As ?? "DynamicMemory";
            }
        }
    }
}
