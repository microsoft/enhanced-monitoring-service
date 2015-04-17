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

namespace EnhancedMonitoring.DataCollector
{
    public class QueryMgmtObject : MgmtObject
    {
        
        private String[] selectedProperties;

        internal QueryMgmtObject(MgmtObjectConfiguration conf) : base(conf)
        {
            //If specific perf count is given, instead of "*", which means select all,
            //    we should construct an array of selected properties and parse it through WQL query to improve performance
            if (this.conf.PerfCounters != null && this.conf.PerfCounters.Count != 0
                    && !this.conf.PerfCounters.Any(counter => !String.IsNullOrEmpty(counter.Select) && counter.Select.Equals("*")))
            {
                //The properties start with "_" needs to be handled specially.
                //Like "_Path", it is not in Properties set, which is by the design of .net's WMI libary.
                this.selectedProperties = this.conf.PerfCounters
                  .Select(counter => counter.Select).Where(p => !String.IsNullOrEmpty(p) && !p.StartsWith("_")).ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">Named arguments used in "Where" field of WQL query</param>
        /// <returns></returns>
        public override Object CollectData(IDictionary<String, Object> args)
        {
            if(args == null)
            {
                throw new ArgumentNullException("args");
            }
            
            if (this.conf.PerfCounters == null)
            {
                return null;
            }

            String condition = String.Empty;

            condition = NamedArgumentHelper.Resolve(this.conf.Where, this.conf.WhereArgs, args);

            if (this.conf.PerfCounters == null || this.conf.PerfCounters.Count == 0)
            {
                return null;
            }

            using (var result = WMIHelper.QueryInstacnes(this.conf.Namespace, this.conf.From, condition, this.selectedProperties))
            {                
                var mgmtObjs = result.Cast<ManagementObject>();
                if(this.conf.ReturnValueType == MgmtObjectReturnValueType.Single)
                {
                    var mgmtObj = mgmtObjs.FirstOrDefault();
                    return SelectPerfCounter(mgmtObj, args);
                }
                else
                {
                    var list = new List<Object>();
                    foreach (var mgmtObj in mgmtObjs)
                    {
                        list.Add(SelectPerfCounter(mgmtObj, args));
                    }
                    return list;
                }
                
            }
        }

        private IDictionary<String, Object> SelectPerfCounter(ManagementObject mgmtObj, IDictionary<String, Object> args)
        {
            IDictionary<String, Object> data = new Dictionary<String, Object>();

            this.conf.PerfCounters.ForEach(counter =>
            {
                if (counter.Select.Equals("*"))
                {
                    foreach (var prop in mgmtObj.Properties)
                    {
                        var key = prop.Name;
                        var val = prop.Value;
                        if (data.ContainsKey(key))
                        {
                            throw new ArgumentException(String.Format("Metric:'{0}' already exists", key));
                        }
                        else
                        {
                            data.Add(key, val);
                        }
                    }
                }
                else if (counter.Select.Equals("_Path"))
                {
                    var key = counter.As ?? "Path";
                    var val = mgmtObj.Path;
                    if (data.ContainsKey(key))
                    {
                        throw new ArgumentException(String.Format("Metric:'{0}' already exists", key));
                    }
                    else
                    {
                        data.Add(key, val);
                    }
                }
                else
                {
                    try
                    {
                        var property = mgmtObj.Properties[counter.Select.Trim()];                        
                        var key = counter.As ?? property.Name;
                        var val = property.Value;
                        if (data.ContainsKey(key))
                        {
                            throw new ArgumentException(String.Format("Metric:'{0}' already exists", key));
                        }
                        else
                        {
                            data.Add(key, val);
                        }
                    }
                    catch (ManagementException e)
                    {
                        throw new ManagementException(String.Format("Property:'{0}' not found", counter.Select), e);
                    }
                }
            });
            return data;
        }        

        public override String KeyName
        {
            get
            {
                return this.conf.From ?? this.conf.As;
            }
        }
    }
}
