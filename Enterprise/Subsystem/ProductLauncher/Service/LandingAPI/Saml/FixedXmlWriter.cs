using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Saml
{
    class FixedXmlWriter : System.Xml.XmlTextWriter
    {
        public FixedXmlWriter(TextWriter tw) : base(tw) { }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            if (ns == "urn:oasis:names:tc:SAML:2.0:assertion")
            {
                base.WriteStartElement("saml", localName, ns);
            }
            else
            {
                base.WriteStartElement(prefix, localName, ns);
            }
        }
    }
}