using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

public class XmlSignCreate
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

            if ( Path.GetExtension(path).ToLower() != ".xml")
            {
                Console.WriteLine("Path to XML file not found: " + path);
                Console.ReadKey();
                return;
            }

            if (Path.GetFileName(path).ToLower() == "license.xml")
            {
                Console.WriteLine("Invalid file name 'license.xml'");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("XML file (unsigned): " + path);

            // Create a new CspParameters object to specify
            // a key container.
            CspParameters cspParams = new CspParameters();
            cspParams.KeyContainerName = "XML_DSIG_RSA_KEY_ANYBELL_C4B";

            // Create a new RSA signing key and save it in the container.
            RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider(cspParams);

            string publicKey = rsaKey.ToXmlString(false);

            // Create a new XML document.
            XmlDocument xmlDoc = new XmlDocument();

            // Load an XML file into the XmlDocument object.
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(path);

            XmlNode licPackage = xmlDoc.SelectSingleNode("LicensePackage");
            string licPackageWithPublicKey = "<LicensePackage>" 
                + licPackage.InnerXml 
                + "<SignatureKey>" 
                + publicKey 
                + "</SignatureKey>" 
                + "</LicensePackage>";
            xmlDoc.LoadXml(licPackageWithPublicKey);

            // Sign the XML document.
            SignXml(xmlDoc, rsaKey);


            // Save the document.
            string licenseFile = Path.Combine(Path.GetDirectoryName(path), "license.xml");
            xmlDoc.Save(licenseFile);

            Console.WriteLine("XML file (signed):   " + licenseFile);
            Console.WriteLine("SUCCESS");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        Console.ReadKey();
    }

    // Sign an XML file.
    // This document cannot be verified unless the verifying
    // code has the key with which it was signed.
    public static void SignXml(XmlDocument xmlDoc, RSA rsaKey)
    {
        // Check arguments.
        if (xmlDoc == null)
            throw new ArgumentException(nameof(xmlDoc));
        if (rsaKey == null)
            throw new ArgumentException(nameof(rsaKey));

        // Create a SignedXml object.
        SignedXml signedXml = new SignedXml(xmlDoc);

        // Add the key to the SignedXml document.
        signedXml.SigningKey = rsaKey;

        // Create a reference to be signed.
        Reference reference = new Reference();
        reference.Uri = "";

        // Add an enveloped transformation to the reference.
        XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
        reference.AddTransform(env);

        // Add the reference to the SignedXml object.
        signedXml.AddReference(reference);

        // Compute the signature.
        signedXml.ComputeSignature();

        // Get the XML representation of the signature and save
        // it to an XmlElement object.
        XmlElement xmlDigitalSignature = signedXml.GetXml();

        // Append the element to the XML document.
        xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));

    }

}