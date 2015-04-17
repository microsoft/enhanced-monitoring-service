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
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EnhancedMonitoring.Configuration
{
    [XmlRoot("MgmtObject")]
    public class MgmtObjectConfiguration
    {
        [XmlAttribute("Type")]
        public MgmtObjectType Type { get; set; }

        [XmlAttribute("SuppressError")]
        public Boolean SuppressError { get; set; }

        [XmlAttribute("ReturnValue")]
        public MgmtObjectReturnValueType ReturnValueType { get; set; }

        [XmlElement("Namespace")]
        public String Namespace { get; set; }

        [XmlElement("From")]
        public String From { get; set; }

        [XmlElement("As")]
        public String As { get; set; }

        [XmlElement("Where")]
        public String Where { get; set; }

        [XmlArray("WhereArgs")]
        [XmlArrayItem("WhereArg")]
        public WhereArgList WhereArgs { get; set; }

        [XmlArray("PerfCounters")]
        [XmlArrayItem("PerfCounter")]
        public PerfCounterConfigurationList PerfCounters { get; set; }

    }

    public class WhereArgList : List<WhereArgConfiguration>
    {

    }

    public class PerfCounterConfigurationList : List<PerfCounterConfiguration>
    {

    }

}
