using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Globalization;
using Microsoft.Win32;

namespace XPhoneLicense
{
    class Program
    {

        static string xpLicenseFileName
        {
            get
            {
                string path = Path.Combine(AppContext.BaseDirectory, "license.xml");
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\C4B\\XPhoneServer"))
                    {
                        if (key != null)
                        {
                            Object o = key.GetValue("InstallDir");
                            if (o != null)
                            {
                                path = o as String;
                                path = Path.Combine(path, "license\\license.xml");
                            }
                        }
                    }
                }
                catch
                {
                }
                return path;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine(xpLicenseFileName);
            if ( !File.Exists(xpLicenseFileName) )
            {
                Console.WriteLine("Datei existiert nicht.");
                Console.ReadLine();
                return;
            }

            XmlDocument xpLicenseXmlDoc = new XmlDocument();
            xpLicenseXmlDoc.Load(xpLicenseFileName);

            string SystemID = "";
            try
            {
                SystemID = xpLicenseXmlDoc.GetElementsByTagName("SystemID")[0].InnerText;
            }
            catch { }
            Console.WriteLine("SystemID:\t" + SystemID);

            string CustomerName = "";
            try
            {
                CustomerName = xpLicenseXmlDoc.GetElementsByTagName("CustomerData")[0].Attributes.GetNamedItem("Name").InnerText;
            }
            catch { }
            Console.WriteLine("CustomerName:\t" + CustomerName);

            //XmlNodeList system = xpLicenseXmlDoc.GetElementsByTagName("SystemID");
            //if ( system != null && system.Count > 0 )
            //{
            //    string s = system[0].InnerText;
            //    Console.WriteLine("SystemID:\t" + s);
            //}

            //XmlNodeList customer = xpLicenseXmlDoc.GetElementsByTagName("CustomerData");
            //if (customer != null && customer.Count > 0)
            //{
            //    string c = customer[0].Attributes.GetNamedItem("Name").InnerText;
            //    Console.WriteLine("CustomerName:\t" + c);
            //}

            XmlNodeList list = xpLicenseXmlDoc.GetElementsByTagName("ProductInfo");
            foreach (XmlNode node in list)
            {
                if ( node.Attributes.GetNamedItem("Code").InnerText == "50" )
                {
                    XmlNode parent = node.ParentNode;
                    string Description = parent?.SelectSingleNode("Description").InnerText;
                    Console.WriteLine("Description:\t" + Description);

                    string ExpirationDate = parent?.SelectSingleNode("ExpirationDate").InnerText;
                    Console.WriteLine("ExpirationDate:\t" + ExpirationDate);

                    string LicenseType = parent?.SelectSingleNode("LicenseType").InnerText;
                    Console.WriteLine("LicenseType:\t" + LicenseType);
                }
            }

            ////Console.WriteLine();

            ////XmlNodeList nodes = xpLicenseXmlDoc.LastChild.LastChild.ChildNodes;
            ////foreach(XmlNode node in nodes )
            ////{
            ////    if (node.Name == "SystemID")
            ////    {
            ////        string SystemID = node.InnerText;
            ////        Console.WriteLine("SystemID:\t" + SystemID);
            ////    }

            ////    if (node.Name == "CustomerData")
            ////    {
            ////        foreach (XmlAttribute x in node.Attributes)
            ////        {
            ////            if (x.Name == "Name")
            ////            {
            ////                string CustomerName = x.Value;
            ////                Console.WriteLine("CustomerName:\t" + CustomerName);
            ////            }
            ////        }
            ////    }

            ////    if ( node.Name == "Signature" )
            ////    {
            ////        bool licFound = false;
            ////        XmlNode nodeObject = node.LastChild.SelectSingleNode("ProductInfo");
            ////        foreach( XmlAttribute x in nodeObject.Attributes)
            ////        {
            ////            if ( x.Name == "Code"  && x.Value == "50")
            ////            {
            ////                //Console.WriteLine("FOUND: XPhone AnyBell License");
            ////                licFound = true;
            ////            }
            ////        }
            ////        if ( licFound )
            ////        {
            ////            string Description = node.LastChild.SelectSingleNode("Description").InnerText;
            ////            Console.WriteLine("Description:\t" + Description);

            ////            string ExpirationDate = node.LastChild.SelectSingleNode("ExpirationDate").InnerText;
            ////            Console.WriteLine("ExpirationDate:\t" + ExpirationDate);
            ////        }
            ////    }
            ////}
            
            Console.ReadLine();
        }
    }
}
