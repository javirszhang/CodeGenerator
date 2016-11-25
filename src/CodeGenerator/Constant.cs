﻿using CodeGenerator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CodeGenerator.Core
{
    public class Constant : IConstant
    {
        public Constant(string @namespace)
        {
            this.Namespace = @namespace;
        }
        public string CurrentTime
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        public string Version
        {
            get
            {
                Assembly ass = Assembly.GetExecutingAssembly();
                object[] attributes = ass.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
                if (attributes.Length == 0)
                {
                    return ass.GetName().Version.ToString();
                }
                else
                {
                    return ((AssemblyFileVersionAttribute)attributes[0]).Version;
                }
            }
        }
        public string Namespace
        {
            get;
            set;
        }
    }
}
