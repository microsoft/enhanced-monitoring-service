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
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMonitoring
{
    public static class WMIHelper
    {

        public static ManagementObject CreateInstance(String scope, String className)
        {
            if (String.IsNullOrEmpty(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }
            if (String.IsNullOrEmpty(className))
            {
                throw new ArgumentNullException(nameof(className));
            }
            var clazz = new ManagementClass(scope, className, null);
            return clazz.CreateInstance();
        }

        public static ManagementObjectCollection GetInstances(String scope, String className)
        {
            if (String.IsNullOrEmpty(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }
            if (String.IsNullOrEmpty(className))
            {
                throw new ArgumentNullException(nameof(className));
            }
            var clazz = new ManagementClass(scope, className, null);
            return clazz.GetInstances();
        }

        public static ManagementObject GetFirstInstance(String scope, String className)
        {
            if (String.IsNullOrEmpty(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }
            if (String.IsNullOrEmpty(className))
            {
                throw new ArgumentNullException(nameof(className));
            }
            return GetInstances(scope, className).Cast<ManagementObject>().FirstOrDefault();
        }
        public static ManagementObjectCollection QueryInstacnes(String scope, String className, String condition)
        {
            if (String.IsNullOrEmpty(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }
            if (String.IsNullOrEmpty(className))
            {
                throw new ArgumentNullException(nameof(className));
            }
            return QueryInstacnes(scope, className, condition, null);
        }
        public static ManagementObjectCollection QueryInstacnes(String scope, String className, String condition, String[] selectedProperties)
        {
            if (String.IsNullOrEmpty(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }
            if (String.IsNullOrEmpty(className))
            {
                throw new ArgumentNullException(nameof(className));
            }
            var query = new SelectQuery(className, condition);
            if(selectedProperties != null)
            {
                var collection = new System.Collections.Specialized.StringCollection();
                collection.AddRange(selectedProperties);
                query.SelectedProperties = collection;
            }
            using (var search = new ManagementObjectSearcher(new ManagementScope(scope), query))
            {
                try
                {
                    var result = search.Get();
                    if (result.Count == 0)
                    {
                        throw new NotImplementedException(String.Format("Can't find wmi object: namespace='{0}', class='{1}', condition='{2}'", 
                                                                            scope, className, condition));
                    }
                    return result;
                }
                catch (ManagementException e)
                {
                    throw new ManagementException(String.Format("Wmi query error: namespace='{0}', class='{1}', condition='{2}'\n", 
                                                                    scope, className, condition), e);
                }
            }
        }

        public static ManagementObject QueryFirstInstacne(String scope, String className, String condition)
        {
            if (String.IsNullOrEmpty(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }
            if (String.IsNullOrEmpty(className))
            {
                throw new ArgumentNullException(nameof(className));
            }
            return QueryFirstInstacne(scope, className, condition, null);
        }

        public static ManagementObject QueryFirstInstacne(String scope, String className, String condition, String[] selectedProperties)
        {
            if (String.IsNullOrEmpty(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }
            if (String.IsNullOrEmpty(className))
            {
                throw new ArgumentNullException(nameof(className));
            }
            return QueryInstacnes(scope, className, condition, selectedProperties).Cast<ManagementObject>().FirstOrDefault();
        }
        static class JobState
        {
            public const UInt16 New = 2;
            public const UInt16 Starting = 3;
            public const UInt16 Running = 4;
            public const UInt16 Suspended = 5;
            public const UInt16 ShuttingDown = 6;
            public const UInt16 Completed = 7;
            public const UInt16 Terminated = 8;
            public const UInt16 Killed = 9;
            public const UInt16 Exception = 10;
            public const UInt16 Service = 11;
        }

        public static void WaitForAsyncJob(ManagementBaseObject outParams, String scope)
        {
            if (outParams == null)
            {
                throw new ArgumentNullException(nameof(outParams));
            }
            if (String.IsNullOrEmpty(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }
            // Retrieve msvc_StorageJob path. This is a full wmi path.
            string JobPath = (string)outParams["Job"];
            var Job = new ManagementObject(new ManagementScope(scope), new ManagementPath(JobPath), null);
            // Try to get storage job information.
            Job.Get();
            while ((UInt16)Job["JobState"] == JobState.Starting
                || (UInt16)Job["JobState"] == JobState.Running)
            {
                Console.WriteLine("In progress... {0}% completed.", Job["PercentComplete"]);
                Task.Delay(1000);
                Job.Get();
            }

            // Figure out if job failed.
            var jobState = (UInt16)Job["JobState"];
            if (jobState != JobState.Completed)
            {
                UInt16 jobErrorCode = (UInt16)Job["ErrorCode"];
                String jobErrorDescription = (string)Job["ErrorDescription"];
                Console.WriteLine("Error Code:{0}", jobErrorCode);
                Console.WriteLine("ErrorDescription: {0}", jobErrorDescription);

                throw new ManagementException(String.Format("Job didn't complete successfully. Error Code={0}, {1}", jobErrorCode, jobErrorDescription));
            }
        }
    }
}
