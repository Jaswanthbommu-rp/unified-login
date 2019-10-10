using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Saml
{
    public class RealPageSAML2
    {
        private IList<ProductInternalSetting> _ProductInternalSettingList = new List<ProductInternalSetting>();

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="SigningCertificate"></param>
        /// <param name="Issuer"></param>
        /// <param name="ProductInternalSettingList"></param>
        public RealPageSAML2(X509Certificate2 SigningCertificate, string Issuer, IList<ProductInternalSetting> ProductInternalSettingList)
        {
            this._Issuer = Issuer;
            this._SigningCertificate = SigningCertificate;
            this._ProductInternalSettingList = ProductInternalSettingList;
        }

        #region Privates
        /// <summary>
        /// 
        /// </summary>
        private string _Destination { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        private string _Issuer { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        private string _Subject { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        private string _TokenIssuer { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        private X509Certificate2 _SigningCertificate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private int _ProductId { get; set; } = 0;

        #endregion

        #region Public

        /// <summary>
        /// Used to set the destination URL for the SAML request
        /// </summary>
        public string Subject
        {
            get
            {
                if (_Subject == null)
                {
                    return "";
                }
                else
                {
                    return _Subject;
                }
            }
            set { _Subject = value; }
        }

        /// <summary>
        /// Used to set the destination URL for the SAML request
        /// </summary>
        public string Destination
        {
            get { return _Destination; }
            set { _Destination = value; }
        }

        public string TokenIssuer
        {
            get { return _TokenIssuer; }
            set { _TokenIssuer = value; }
        }

        public int ProductId
        {
            get { return _ProductId; }
            set { _ProductId = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public IList<SamlAttributes> AttributeList;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public XmlDocument BuildAssertion()
        {

            DateTime issueInstant = DateTime.UtcNow.ToUniversalTime();

            // build the SAML assertion
            Saml2Assertion assertion = new Saml2Assertion(new Saml2NameIdentifier(_Issuer, new Uri(RealPageSAML2.AssertionUri)));

            assertion.Subject = new Saml2Subject(new Saml2NameIdentifier(_Subject, new Uri(RealPageSAML2.NameIDFormatUris.Unspecified)));

            // SalesForce required SAML info
            if (_ProductId == (int)ProductEnum.ClientPortal)
            {
                assertion.Subject = new Saml2Subject(new Saml2NameIdentifier(_Subject, new Uri(RealPageSAML2.NameIDFormatUris.Email)));
                Saml2SubjectConfirmation conf = new Saml2SubjectConfirmation(new Uri("urn:oasis:names:tc:SAML:2.0:cm:bearer"));
                Saml2AudienceRestriction audience = new Saml2AudienceRestriction();
                audience.Audiences.Add(new Uri("https://saml.salesforce.com"));
                assertion.Conditions = new Saml2Conditions();
                assertion.Conditions.AudienceRestrictions.Add(audience);
                conf.NameIdentifier = new Saml2NameIdentifier(_Subject);
                conf.NameIdentifier.Format = new Uri(RealPageSAML2.NameIDFormatUris.Unspecified);
                conf.SubjectConfirmationData = new Saml2SubjectConfirmationData();
                string recipient = _ProductInternalSettingList.First(a => a.Name.ToUpper() == "SAMLRECIPIENT").Value;
                conf.SubjectConfirmationData.Recipient = new Uri(recipient);
                conf.SubjectConfirmationData.NotOnOrAfter = DateTime.UtcNow.AddHours(1);
                assertion.Subject.SubjectConfirmations.Add(conf);
            }
            // SalesForce required SAML info

            assertion.Id = new Saml2Id();
            assertion.IssueInstant = issueInstant;
            assertion.Issuer = new Saml2NameIdentifier(_Issuer);

            assertion.Conditions = new Saml2Conditions();
            assertion.Conditions.NotBefore = DateTime.UtcNow.AddHours(-1);
            assertion.Conditions.NotOnOrAfter = DateTime.UtcNow.AddHours(1);

            // SalesForce required SAML info
            if (_ProductId == (int)ProductEnum.ClientPortal)
            {
                Saml2AudienceRestriction ar = new Saml2AudienceRestriction(new Uri("https://saml.salesforce.com"));
                assertion.Conditions.AudienceRestrictions.Add(ar);
            }
            // SalesForce required SAML info

            Saml2AuthenticationStatement authn = new Saml2AuthenticationStatement(new Saml2AuthenticationContext(new Uri(RealPageSAML2.PasswordUri)));
            authn.SessionIndex = Guid.NewGuid().ToString();
            assertion.Statements.Add(authn);

            List<Saml2Attribute> samlAttributes = new List<Saml2Attribute>();

            foreach (SamlAttributes attr in AttributeList)
            {
                samlAttributes.Add(new Saml2Attribute(attr.Name, attr.Value) { NameFormat = new Uri(attr.Type), FriendlyName = attr.Name });
            }

            Saml2AttributeStatement attrstatement = new Saml2AttributeStatement(samlAttributes);
            assertion.Statements.Add(attrstatement);

            X509SigningCredentials clientSigningCredentials = new X509SigningCredentials(_SigningCertificate, RealPageSAML2.Algorithms.SHA1_SignatureMethod, RealPageSAML2.Algorithms.SHA1_DigestMethod);
            assertion.SigningCredentials = clientSigningCredentials;

            Saml2SecurityToken stoken = new Saml2SecurityToken(assertion);
            Saml2SecurityTokenHandler handler = new Saml2SecurityTokenHandler();
            SecurityTokenDescriptor desc = new SecurityTokenDescriptor();
            desc.Token = stoken;
            desc.TokenIssuerName = _TokenIssuer;

            var sw = new StringWriter();

            // use the Saml2Assertion to build the Assertion XML object, but sign the assertion later once it can include the correct namespaces
            handler.WriteToken(new XmlTextWriter(sw) { Namespaces = true }, stoken);
            XmlDocument assertionXMLDocument = new XmlDocument();
            assertionXMLDocument.LoadXml(sw.ToString());

            // add the saml prefix to the namespaces in the document
            AddPrefix(assertionXMLDocument.DocumentElement, "saml");

            XmlDocument responseXMLDocument = new XmlDocument();
            XmlElement responseXmlElement = responseXMLDocument.CreateElement(RealPageSAML2.Prefixes.SAMLP, "Response", RealPageSAML2.NamespaceURIs.Protocol);

            string responseSaml2Id = new Saml2Id().Value;
            responseXmlElement.SetAttribute("ID", responseSaml2Id);
            responseXmlElement.SetAttribute("Version", RealPageSAML2.Version);
            responseXmlElement.SetAttribute("IssueInstant", issueInstant.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            responseXmlElement.SetAttribute("Destination", _Destination);
            XmlElement issuerXmlElement = responseXMLDocument.CreateElement(RealPageSAML2.Prefixes.SAML, "Issuer", RealPageSAML2.NamespaceURIs.Assertion);
            issuerXmlElement.InnerText = _Issuer;
            responseXmlElement.AppendChild(issuerXmlElement);

            XmlElement statusXmlElement = responseXMLDocument.CreateElement(RealPageSAML2.Prefixes.SAMLP, "Status", RealPageSAML2.NamespaceURIs.Protocol);
            XmlElement statusCodeXmlElement = responseXMLDocument.CreateElement(RealPageSAML2.Prefixes.SAMLP, "StatusCode", RealPageSAML2.NamespaceURIs.Protocol);
            statusCodeXmlElement.SetAttribute("Value", RealPageSAML2.StatusUris.Success);
            statusXmlElement.AppendChild(statusCodeXmlElement);
            responseXmlElement.AppendChild(statusXmlElement);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(assertionXMLDocument.NameTable);
            nsmgr.AddNamespace("sig", RealPageSAML2.NamespaceURIs.Signature);
            XmlNode signature = assertionXMLDocument.SelectSingleNode("//sig:Signature", nsmgr);
            // remove the signature created by the Saml2Assertion WriteToken process because the xml being signed isn't in the correct form for products to accecpt it
            assertionXMLDocument.DocumentElement.RemoveChild(signature);

            XmlNode importAssertion = responseXmlElement.OwnerDocument.ImportNode(assertionXMLDocument.DocumentElement, true);
            responseXmlElement.AppendChild(importAssertion);
            responseXMLDocument.AppendChild(responseXmlElement);

            // begin new signing procedure for the SAML assertion xml
            Reference reference = new Reference();
            reference.Uri = "#" + responseSaml2Id;
            SignedXml signedXml = new SignedXml(responseXMLDocument); // the entire document

            signedXml.SigningKey = _SigningCertificate.PrivateKey;
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;

            //canonicalize
            XmlDsigExcC14NTransform e14t = new XmlDsigExcC14NTransform("#default samlp saml ds xs xsi");
            XmlDsigEnvelopedSignatureTransform envT = new XmlDsigEnvelopedSignatureTransform(false);
            reference.AddTransform(envT); // add first
            reference.AddTransform(e14t);

            KeyInfo keyInfo = new KeyInfo();
            KeyInfoX509Data keyInfoData = new KeyInfoX509Data(_SigningCertificate);
            KeyInfoName kin = new KeyInfoName();
            RSACryptoServiceProvider rsaprovider = (RSACryptoServiceProvider)_SigningCertificate.PublicKey.Key;
            RSAKeyValue rkv = new RSAKeyValue(rsaprovider);
            keyInfo.AddClause(keyInfoData);
            signedXml.KeyInfo = keyInfo;

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Compute the signature.
            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save 
            // it to an XmlElement object.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            responseXMLDocument.DocumentElement.InsertBefore(responseXMLDocument.ImportNode(xmlDigitalSignature, true), importAssertion);

            // test to see if the result xml can be validated
            /*
            SignedXml testSignedXml = new SignedXml(responseXMLDocument);
            XmlNamespaceManager nsmgr1 = new XmlNamespaceManager(assertionXMLDocument.NameTable);
            nsmgr1.AddNamespace("sig", RealPageSAML2.NamespaceURIs.Signature);
            XmlElement sig = responseXMLDocument.SelectSingleNode("//sig:Signature", nsmgr1) as XmlElement;
            testSignedXml.LoadXml(sig);
            if (!testSignedXml.CheckSignature(_SigningCertificate, true))
            {
                throw new Exception("signature check failed");
            }
            */

            return responseXMLDocument;
        }

        #endregion

        /// <summary>
        /// Used to add the xml namespace to the prefix of the node
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="prefix"></param>
        private void AddPrefix(XmlNode xn, string prefix)
        {
            if (xn.HasChildNodes)
            {
                for (int i = 0; i < xn.ChildNodes.Count; i++)
                {
                    AddPrefix(xn.ChildNodes[i], prefix);
                }
                xn.Prefix = prefix;
            }
            else
            {
                xn.Prefix = prefix;
            }
        }

        /// <summary>
        /// The SAML version
        /// </summary>
        public const string Version = "2.0";

        /// <summary>
        /// The Assertion URI
        /// </summary>
        public const string AssertionUri = "urn:oasis:names:tc:SAML:2.0:assertion";

        /// <summary>
        /// The Password URI
        /// </summary>
        public const string PasswordUri = "urn:oasis:names:tc:SAML:2.0:ac:classes:Password";

        /// <summary>
        /// The SAML XML prefixes
        /// </summary>
        public static class Prefixes
        {
            /// <summary>
            /// The SAML prefix
            /// </summary>
            public const string SAML = "saml";

            /// <summary>
            /// The SAML protocol prefix
            /// </summary>
            public const string SAMLP = "samlp";

            /// <summary>
            /// The SAML metadata prefix
            /// </summary>
            public const string MD = "md";
        }

        /// <summary>
        /// The SAML XML namespace URIs
        /// </summary>
        public static class NamespaceURIs
        {
            /// <summary>
            /// The SAML assertion namespace URI
            /// </summary>
            public const string Assertion = "urn:oasis:names:tc:SAML:2.0:assertion";

            /// <summary>
            /// The SAML protocol namespace URI
            /// </summary>
            public const string Protocol = "urn:oasis:names:tc:SAML:2.0:protocol";

            /// <summary>
            /// The SAML metadata namespace URI
            /// </summary>
            public const string Metadata = "urn:oasis:names:tc:SAML:2.0:metadata";

            /// <summary>
            /// The SAML encryption signature namespage
            /// </summary>
            public const string Signature = "http://www.w3.org/2000/09/xmldsig#";
        }
        public static class AttributeURIs
        {
            /// <summary>
            /// The SAML basic attribute URI
            /// </summary>
            public const string Basic = "urn:oasis:names:tc:SAML:2.0:attrname-format:basic";
            /// <summary>
            /// The SAML dotnet attribute URI
            /// </summary>
            public const string DotNet = "urn:oasis:names:tc:SAML:2.0:attrname-format:dotnet";

        }

        public static class StatusUris
        {
            /// <summary>
            /// The SAML status Success URI
            /// </summary>
            public const string Success = "urn:oasis:names:tc:SAML:2.0:status:Success";
        }

        public static class NameIDFormatUris
        {
            /// <summary>
            /// The SAML NameID format unspecified URI
            /// </summary>
            public const string Unspecified = "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified";
            /// <summary>
            /// The SAML NameID format email URI
            /// </summary>
            public const string Email = "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress";
        }

        public static class Algorithms
        {
            /// <summary>
            /// The SAML SHA1 signature method URI
            /// </summary>
            public const string SHA1_SignatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
            /// <summary>
            /// The SAML SHA1 digest method URI
            /// </summary>
            public const string SHA1_DigestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";
        }
    }
}
