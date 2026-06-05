using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalScepMdmProfile
    {
        private const string SyncMlMetInfNamespace = "syncml:metinf";

        public string UniqueId { get; set; }
        public string Scope { get; set; }

        public string ServerUrl { get; set; }
        public string Challenge { get; set; }

        public string SubjectName { get; set; }
        public string SubjectAlternativeNames { get; set; }
        public string EkuMapping { get; set; }
        public int? KeyUsage { get; set; }

        public int? KeyLength { get; set; }
        public string KeyAlgorithm { get; set; }
        public string HashAlgorithm { get; set; }
        public int? KeyProtection { get; set; }
        public string ContainerName { get; set; }

        public string ValidPeriod { get; set; }
        public int? ValidPeriodUnits { get; set; }
        public int? RetryCount { get; set; }
        public int? RetryDelay { get; set; }

        public string TemplateName { get; set; }
        public string CAThumbprint { get; set; }
        public string CustomTextToShowInPrompt { get; set; }

        public string SourceProfileId { get; set; }
        public string SourceProfileSlug { get; set; }

        public string ToSyncMl()
        {
            if (string.IsNullOrEmpty(UniqueId)) { throw new InvalidOperationException("UniqueId is required."); }
            if (string.IsNullOrEmpty(ServerUrl)) { throw new InvalidOperationException("ServerUrl is required."); }

            string scopeSegment = string.Equals(Scope, "User", StringComparison.OrdinalIgnoreCase) ? "./User" : "./Device";
            string nodeBase = string.Concat(scopeSegment, "/Vendor/MSFT/ClientCertificateInstall/SCEP/", UniqueId, "/Install/");

            List<CspNode> nodes = new List<CspNode>();
            AddString(nodes, "ServerURL", ServerUrl);
            AddString(nodes, "Challenge", Challenge);
            AddString(nodes, "SubjectName", SubjectName);
            AddString(nodes, "SubjectAlternativeNames", SubjectAlternativeNames);
            AddString(nodes, "EKUMapping", EkuMapping);
            AddInt(nodes, "KeyUsage", KeyUsage);
            AddInt(nodes, "KeyLength", KeyLength);
            AddString(nodes, "KeyAlgorithm", KeyAlgorithm);
            AddString(nodes, "HashAlgorithm", HashAlgorithm);
            AddInt(nodes, "KeyProtection", KeyProtection);
            AddString(nodes, "ContainerName", ContainerName);
            AddString(nodes, "ValidPeriod", ValidPeriod);
            AddInt(nodes, "ValidPeriodUnits", ValidPeriodUnits);
            AddInt(nodes, "RetryCount", RetryCount);
            AddInt(nodes, "RetryDelay", RetryDelay);
            AddString(nodes, "TemplateName", TemplateName);
            AddString(nodes, "CAThumbprint", CAThumbprint);
            AddString(nodes, "CustomTextToShowInPrompt", CustomTextToShowInPrompt);

            XDocument document = new XDocument(new XDeclaration("1.0", "utf-8", null));
            XElement syncBody = new XElement("SyncBody");
            XElement atomic = new XElement("Atomic", new XElement("CmdID", "1"));

            int cmdId = 2;
            foreach (CspNode node in nodes)
            {
                XElement meta = new XElement("Meta", new XElement(XName.Get("Format", SyncMlMetInfNamespace), node.Format));
                XElement item = new XElement("Item",
                    new XElement("Target", new XElement("LocURI", string.Concat(nodeBase, node.Suffix))),
                    meta,
                    new XElement("Data", node.Value));
                atomic.Add(new XElement("Replace", new XElement("CmdID", cmdId.ToString(System.Globalization.CultureInfo.InvariantCulture)), item));
                cmdId++;
            }

            XElement enrollItem = new XElement("Item",
                new XElement("Target", new XElement("LocURI", string.Concat(nodeBase, "Enroll"))),
                new XElement("Meta", new XElement(XName.Get("Format", SyncMlMetInfNamespace), "node")));
            atomic.Add(new XElement("Exec", new XElement("CmdID", cmdId.ToString(System.Globalization.CultureInfo.InvariantCulture)), enrollItem));

            syncBody.Add(atomic);
            document.Add(syncBody);

            XmlWriterSettings writerSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineHandling = NewLineHandling.Replace,
                Encoding = new UTF8Encoding(false),
                OmitXmlDeclaration = false,
                CloseOutput = false
            };

            string serialized;
            using (MemoryStream buffer = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(buffer, writerSettings))
                {
                    document.Save(writer);
                }
                serialized = writerSettings.Encoding.GetString(buffer.ToArray());
            }

            using (StringReader stringReader = new StringReader(serialized))
            {
                XmlReaderSettings readerSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
                using (XmlReader reader = XmlReader.Create(stringReader, readerSettings))
                {
                    XDocument.Load(reader);
                }
            }

            return serialized;
        }

        private static void AddString(List<CspNode> nodes, string suffix, string value)
        {
            if (string.IsNullOrEmpty(value)) { return; }
            nodes.Add(new CspNode { Suffix = suffix, Value = value, Format = "chr" });
        }

        private static void AddInt(List<CspNode> nodes, string suffix, int? value)
        {
            if (!value.HasValue) { return; }
            nodes.Add(new CspNode { Suffix = suffix, Value = value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture), Format = "int" });
        }

        private sealed class CspNode
        {
            public string Suffix;
            public string Value;
            public string Format;
        }
    }
}
