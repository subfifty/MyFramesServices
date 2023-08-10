﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Globalization;
using Microsoft.Win32;
using System.Reflection;

namespace XPhoneRestApi
{
    public sealed class ApiConfig
    {
        private static ApiConfig instance = null;
        private static readonly object padlock = new object();

        private XmlDocument apiConfigDoc = null;

        public static ApiConfig Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ApiConfig();
                    }
                    return instance;
                }
            }
        }

        ApiConfig()
        {
            apiConfigDoc = new XmlDocument();
            apiConfigDoc.PreserveWhitespace = true;
            apiConfigDoc.Load(ConfigFileName);
            //ReloadConfiguration();
        }

        internal void ReloadConfiguration()
        {
            return;

            try
            {
                //lock (padlock)
                {
                    //apiConfigDoc = new XmlDocument();
                    apiConfigDoc.PreserveWhitespace = true;
                    apiConfigDoc.Load(ConfigFileName);
                }
            }
            catch //(Exception ex)
            {
                //apiConfigDoc = new XmlDocument();
            }
        }

        public string ReadAttributeValue(string path, string attribute, string defaultValue = "")
        {
            try
            {
                lock(padlock) 
                {
                    XmlNode node = apiConfigDoc.SelectSingleNode("configuration/" + path);
                    return node.Attributes[attribute].Value;
                }
            }
            catch
            {
                return defaultValue;
            }
        }
        
        private string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().Location;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        internal string ConfigFileName
        {
            get
            {
#if DEBUG
                string path = Path.Combine(AssemblyDirectory, @"ApiConfig\config.xml");
#else
                string path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"C4B\ApiConfig\config.xml");
#endif
                return path;
            }
        }
    }
}
