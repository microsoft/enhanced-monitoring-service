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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace EnhancedMonitoring.DataFormat
{
    public static class DataFormatHelper
    {

        public const String ChunkCountKey = "all";
        public const String ChunkKey = "data";
        public const String TimestampKey = "ts";

        public static String ToJSON(IDictionary<String, Object> data)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            String jsonStr = jsSerializer.Serialize(data);
            return jsonStr;
        }

        public static String ToXml(IDictionary<String, Object> data)
        {
            return DictionaryToXElement("Data", data).ToString();
        }

        private static XElement DictionaryToXElement(String nodeName, IDictionary<String, Object> data)
        {
            nodeName = NormalizeNodeName(nodeName);
            XElement node = new XElement(nodeName);
            foreach (var entry in data)
            {
                node.Add(ObjectToXElement(entry.Key, entry.Value));
            }
            return node;
        }
        
        private static XElement ObjectToXElement(String nodeName, Object nodeContent)
        {
            nodeName = NormalizeNodeName(nodeName);
            if (nodeContent is IList)
            {
                return ListToXElement(nodeName, nodeContent as IList);
            }
            else if (nodeContent is IDictionary<String, Object>)
            {
                return DictionaryToXElement(nodeName, nodeContent as IDictionary<String, Object>);
            }
            else if(nodeContent == null)
            {
                return new XElement(nodeName, "null");
            }
            else
            {
                return new XElement(nodeName, nodeContent);
            }
        }

        private static XElement ListToXElement(String nodeName, IList list)
        {
            nodeName = NormalizeNodeName(nodeName);
            XElement node = new XElement(nodeName);
            node.SetAttributeValue("Type", "List");
            foreach(var elem in list)
            {
                node.Add(ObjectToXElement("Item", elem));
            }
            return node;
        }
        private static string NormalizeNodeName(string nodeName)
        {
            if (String.IsNullOrEmpty(nodeName))
            {
                throw new ArgumentException("Node name can't be null or empty.");
            }
            return nodeName.Trim().Replace(' ', '_');
        }

        public static String Base64Encode(String str)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(str);
            String encoded = System.Convert.ToBase64String(plainTextBytes);
            return encoded;
        }

        public static IList<String> PackString(String str, int blockLength)
        {
            var packed = new List<String>();
            var textBytes = System.Text.Encoding.UTF8.GetBytes(str);

            int i = 0;
            while (i < textBytes.Length)
            {
                packed.Add(System.Text.Encoding.UTF8.GetString(textBytes, i, Math.Min(blockLength, textBytes.Length - i)));
                i += blockLength;
            }
            return packed;
        }

        public static IDictionary<String, Object> ToChunk(String keyPrefix, IList<String> dataChunks, 
                Func<Dictionary<String, Object>, String> baseFormater)
        {
            var packedData = new Dictionary<String, Object>();

            var timestamp = GetCurrentTime();

            for (int i = 0; i < dataChunks.Count; i++)
            {
                var dataChunk = new Dictionary<String, Object>();
                dataChunk.Add(TimestampKey, timestamp);
                dataChunk.Add(ChunkCountKey, dataChunks.Count);
                dataChunk.Add(ChunkKey, dataChunks[i]);

                packedData.Add(String.Format("{0}{1}", keyPrefix, i), baseFormater(dataChunk));
            }

            return packedData;
        }

        public static double GetCurrentTime()
        {
            //To UNIX like timestamp. Milliseconds since 1970/1/1 00:00:00 UTC
            return DateTime.UtcNow
                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .TotalMilliseconds;
        }
    }
}
