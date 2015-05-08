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

namespace EnhancedMonitoring.DataFormat
{
    public class XmlDataFormatter
    {
        protected const Int32 DEFAULT_VALUE_LENGTH = 800;

        private Configuration.MonitorConfiguration conf;

        public XmlDataFormatter(Configuration.MonitorConfiguration monitorConfiguration)
        {
            this.conf = monitorConfiguration;
        }

        public IDictionary<String, Object> ToXml(IDictionary<String, Object> data)
        {
            var dataStr = DataFormatHelper.ToXml(data);
            dataStr = DataFormatHelper.Base64Encode(dataStr);
            var dataChunks = DataFormatHelper.PackString(dataStr, this.conf.MaxValueLength <= 0 ? DEFAULT_VALUE_LENGTH : this.conf.MaxValueLength);
            var packedData = DataFormatHelper.ToChunk(this.conf.DataItemKeyPrefix, dataChunks, DataFormatHelper.ToXml);
            return packedData;
        }
    }
}