using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

public class XmlSignVerify
{
    public static void Main(String[] args)
    {
        try
        {
            string[] cmdArgs = Environment.GetCommandLineArgs();

            string path = "";
            if (cmdArgs.Length > 1)
            {
                path = cmdArgs[1];
            }

            if (Path.GetExtension(path).ToLower() != ".xml")
            {
                Console.WriteLine("Path to XML file not found: " + path);
                Console.ReadKey();
                return;
            }

            // Create a new XML document.
            XmlDocument xmlDoc = new XmlDocument();

            // Load an XML file into the XmlDocument object.
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(path);

            // Verify the signature of the signed XML.
            Console.WriteLine("Verifying signature of: " + path);
            bool result = VerifyXml(xmlDoc);

            // Display the results of the signature verification to the console.
            if (result)
            {
                Console.WriteLine("The XML signature is valid.");
            }
            else
            {
                Console.WriteLine("The XML signature is not valid.");
            }
        }
        catch // (Exception e)
        {
            Console.WriteLine("The XML signature is not valid.");
            //Console.WriteLine(e.Message);
        }

        Console.ReadKey();
    }


    // Verify the signature of an XML file against an asymmetric
    // algorithm and return the result.
    public static Boolean VerifyXml(XmlDocument xmlDoc)
    {
        // Check arguments.
        if (xmlDoc == null)
            throw new ArgumentException("xmlDoc");

            XmlNode pubKey = xmlDoc.SelectSingleNode("LicensePackage/SignatureKey");
            string publicRsaKey = pubKey.InnerXml;

            //XmlDocument publicKey = new XmlDocument();
            //publicKey.Load("rsaPublicKey.xml");
            //string publicRsaKey = publicKey.InnerXml;

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
        return signedXml.CheckSignature(key);
    }
}