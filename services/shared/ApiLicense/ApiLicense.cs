using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Globalization;
using Microsoft.Win32;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace XPhoneRestApi
{
    public sealed class ApiLicense
    {
        private static ApiLicense instance = null;
        private static readonly object padlock = new object();
        
        public static ApiLicense Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ApiLicense();
                    }
                    return instance;
                }
            }
        }

        public LicenseCustomer CustomerInfo { get { return customer; } }
        public LicensePartner PartnerInfo { get { return partner; } }
        public LicensePackage PackageInfo { get { return package; } }

        private XmlDocument licenseXmlDoc = new XmlDocument();
        private XmlDocument xpLicenseXmlDoc = new XmlDocument();

        private bool validSignature = false;

        ApiLicense()
        {
            try
            {
                xpLicenseXmlDoc.PreserveWhitespace = true;
                xpLicenseXmlDoc.Load(xpLicenseFileName);

                ParseLicensePackage();
                ParseLicenseCustomer();
                ParseLicensePartner();
            }
            catch // (Exception ex)
            {
                xpLicenseXmlDoc = new XmlDocument();
            }

            try
            {
                licenseXmlDoc.PreserveWhitespace = true;
                licenseXmlDoc.Load(LicenseFileName);
                if ( VerifyXml(licenseXmlDoc) )
                {
                    validSignature = true;
                    ParseLicensePackage();
                    ParseLicenseCustomer();
                    ParseLicensePartner();
                }
            }
            catch
            {
                licenseXmlDoc = new XmlDocument();
            }

        }

        public LicenseObject ParseLicenseObjectAnyBell()
        {
            LicenseObject licObject = new LicenseObject();

            XmlNodeList list = xpLicenseXmlDoc.GetElementsByTagName("ProductInfo");
            foreach (XmlNode node in list)
            {
                if (node.Attributes.GetNamedItem("Code").InnerText == "50")
                {
                    XmlNode parent = node.ParentNode;
                    //string Description = parent?.SelectSingleNode("Description").InnerText;
                    //Console.WriteLine("Description:\t" + Description);

                    //string ExpirationDate = parent?.SelectSingleNode("ExpirationDate").InnerText;
                    //Console.WriteLine("ExpirationDate:\t" + ExpirationDate);

                    //string LicenseType = parent?.SelectSingleNode("LicenseType").InnerText;
                    //Console.WriteLine("LicenseType:\t" + LicenseType);

                    licObject.description = parent?.SelectSingleNode("Description").InnerText;
                    licObject.expirationdate = parent?.SelectSingleNode("ExpirationDate").InnerText;
                    licObject.version = parent?.SelectSingleNode("Version").InnerText;
                    licObject.type = parent?.SelectSingleNode("LicenseType").InnerText;

                    // Parse date and time with custom specifier.
                    try
                    {
                        if ( String.IsNullOrEmpty(licObject.expirationdate) && licObject.type.ToLower() != "test" && licObject.type.ToLower() != "demo")
                        {
                            licObject.expirationdate = "9/9/9999";
                            licObject.valid = true;
                        }
                        else
                        {
                            DateTime expirationDate = DateTime.ParseExact(licObject.expirationdate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                            licObject.valid = DateTime.Now < expirationDate;
                        }
                    }
                    catch (FormatException)
                    {
                        licObject.valid = false;
                    }
                    
                    break;
                }
            }

            return licObject;
        }

        public LicenseObject ParseLicenseObject(string id)
        {
            LicenseObject licObject = new LicenseObject();

            if (id.ToLower() == "anybell")
            {
                licObject = ParseLicenseObjectAnyBell();
            }

            if (licObject.valid == false)
            {
                XmlNode node = licenseXmlDoc.SelectSingleNode("LicensePackage/Object[@Id='" + id + "']");
                if (node != null)
                {
                    licObject.description = node.SelectSingleNode("Description")?.InnerText;
                    licObject.expirationdate = node.SelectSingleNode("ExpirationDate")?.InnerText;
                    licObject.version = node.SelectSingleNode("Version")?.InnerText;
                    licObject.type = node.SelectSingleNode("LicenseType")?.InnerText;

                    if ( validSignature )
                    {
                        // Parse date and time with custom specifier.
                        try
                        {
                            DateTime expirationDate = DateTime.ParseExact(licObject.expirationdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            licObject.valid = DateTime.Now < expirationDate;
                        }
                        catch (FormatException)
                        {
                            licObject.valid = false;
                        }
                    }
                }
            }

            return licObject;
        }

        private LicenseCustomer customer = new LicenseCustomer();
        private void ParseLicenseCustomer()
        {
            XmlNode node = licenseXmlDoc.SelectSingleNode("LicensePackage/Customer");
            if (node != null)
            {
                customer.name = node.Attributes?.GetNamedItem("Name")?.Value;
                customer.address = node.Attributes?.GetNamedItem("Address")?.Value;
                customer.zipcode = node.Attributes?.GetNamedItem("ZIPCode")?.Value;
                customer.city = node.Attributes?.GetNamedItem("City")?.Value;
                customer.email = node.Attributes?.GetNamedItem("EMail")?.Value;
                customer.country = node.Attributes?.GetNamedItem("Country")?.Value;
            }

            try
            {
                customer.name = xpLicenseXmlDoc.GetElementsByTagName("CustomerData")[0].Attributes.GetNamedItem("Name").InnerText;
            }
            catch { }
        }

        private LicensePartner partner = new LicensePartner();
        private void ParseLicensePartner()
        {
            XmlNode node = licenseXmlDoc.SelectSingleNode("LicensePackage/Partner");
            if (node != null)
            {
                partner.name = node.Attributes?.GetNamedItem("Name")?.Value;
                partner.address = node.Attributes?.GetNamedItem("Address")?.Value;
                partner.zipcode = node.Attributes?.GetNamedItem("ZIPCode")?.Value;
                partner.city = node.Attributes?.GetNamedItem("City")?.Value;
                partner.email = node.Attributes?.GetNamedItem("EMail")?.Value;
                partner.country = node.Attributes?.GetNamedItem("Country")?.Value;
            }

            try
            {
                partner.systemId = xpLicenseXmlDoc.GetElementsByTagName("SystemID")[0].InnerText;
            }
            catch { }
        }

        private LicensePackage package = new LicensePackage();
        private void ParseLicensePackage()
        {
            XmlNode node = licenseXmlDoc.SelectSingleNode("LicensePackage/Package");
            if (node != null)
            {
                package.id = node.Attributes?.GetNamedItem("Id")?.Value;
                package.version = node.Attributes?.GetNamedItem("Version")?.Value;
                package.creationTimeUTC = node.Attributes?.GetNamedItem("CreationTimeUTC")?.Value;
            }

            try
            {
                package.systemId = xpLicenseXmlDoc.GetElementsByTagName("SystemID")[0].InnerText;
            }
            catch { }
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

        internal string LicenseFileName
        {
            get
            {
#if DEBUG
                string path = Path.Combine(AssemblyDirectory, @"Shared_ApiLicense\license.xml");
                if (!File.Exists(path))
                {
                    path = @"D:\SUBFIFTY\MyFramesServices\services\shared\ApiLicense\license.xml";
                }
#else

                string path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"C4B\ApiLicense\license.xml");
#endif
                return path;
            }
        }

        internal string xpLicenseFileName
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
                                path = Path.Combine(path, "licenses\\license.xml");
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

        // Verify the signature of an XML file against an asymmetric
        // algorithm and return the result.
        private Boolean VerifyXml(XmlDocument xmlDoc)
        {
            // Check arguments.
            if (xmlDoc == null)
                throw new ArgumentException("xmlDoc");

            XmlNode pubKey = xmlDoc.SelectSingleNode("LicensePackage/SignatureKey");
            string publicRsaKey = pubKey.InnerXml;

            RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider();
            rsaKey.FromXmlString(publicRsaKey);

            RSA key = rsaKey;

            // Create a new SignedXml object and pass it
            // the XML document class.
            SignedXml signedXml = new SignedXml(xmlDoc);

            // Find the "Signature" node and create a new
            // XmlNodeList object.
            XmlNodeList nodeList = xmlDoc.GetElementsByTagName("Signature");

            // Throw an exception if no signature was found.
            if (nodeList.Count <= 0)
            {
                throw new CryptographicException("Verification failed: No Signature was found in the document.");
            }

            // This example only supports one signature for
            // the entire XML document.  Throw an exception
            // if more than one signature was found.
            if (nodeList.Count >= 2)
            {
                throw new CryptographicException("Verification failed: More that one signature was found for the document.");
            }

            // Load the first <signature> node.
            signedXml.LoadXml((XmlElement)nodeList[0]);

            // Check the signature and return the result.
            bool valid = signedXml.CheckSignature(key);
            return valid;
        }

    }

    public class LicensePackage
    {
        public string id { get; set; }
        public string version { get; set; }
        public string creationTimeUTC { get; set; }
        public string location { get { return ApiLicense.Instance.LicenseFileName; } }
        public string systemId { get; set; }
    }

    public class LicensePartner
    {
        public string name { get; set; }
        public string address { get; set; }
        public string zipcode { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string id { get; set; }
        public string systemId { get; set; }
    }

    public class LicenseCustomer
    {
        public string name { get; set; }
        public string address { get; set; }
        public string zipcode { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string id { get; set; }
    }

    public class LicenseObject
    {
        public LicenseObject()
        {
            valid = false;
        }
        public bool valid { get; set; }
        public string description { get; set; }
        public string expirationdate { get; set; }
        public string type { get; set; }
        public string version { get; set; }
    }

}
