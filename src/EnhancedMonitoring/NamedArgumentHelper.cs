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
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMonitoring
{
    public static class NamedArgumentHelper
    {
        public static String Resolve(String str, IList<WhereArgConfiguration> whereArgs, IDictionary<String, Object> args)
        {
            if (String.IsNullOrEmpty(str))
            {
                return str;
            }

            if(whereArgs == null || whereArgs.Count == 0)
            {
                try 
                {
                    return String.Format(str, new Object[0]);
                }
                catch(FormatException e)
                {
                    throw new ArgumentException(String.Format("format={0}, args=NullOrEmpty", str), e);
                }                
            }

            if(args == null && whereArgs != null && whereArgs.Count > 0)
            {
                throw new ArgumentException(String.Format("Can't resolve argument: format={0}, args={1}", str, String.Join(",", whereArgs.Select(a => a.Name))));
            }

            Object[] argsVal = new Object[args.Count];

            for (int i = 0; i < whereArgs.Count; i++)
            {
                var whereArg = whereArgs[i];
                if (args.ContainsKey(whereArg.Name))
                {
                    argsVal[i] = args[whereArg.Name];
                    if(whereArg.Escape && argsVal[i] != null)
                    {
                        argsVal[i] = WMIQueryHelper.EscapeLikeCondition(argsVal[i].ToString());
                    }
                }
                else
                {
                    throw new ArgumentException(String.Format("Can't resolve argument format={0}, args={1}", str, whereArgs[i].Name));
                }                
            }

            try
            {
                return String.Format(str, argsVal);
            }
            catch(FormatException e)
            {
                throw new ArgumentException(String.Format("Can't resolve argument: format={0}, args={1}", str, String.Join(",", whereArgs)), e);
            }
        }
    }
}
