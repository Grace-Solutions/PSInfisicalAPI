using System.Collections.Generic;
using System.IO;
using System.Xml;
using PSInfisicalAPI.Errors;

namespace PSInfisicalAPI.Imports
{
    public sealed class XmlInfisicalImporter : IInfisicalImporter
    {
        public IList<KeyValuePair<string, string>> Import(FileInfo path)
        {
            if (path == null) { throw new InfisicalImportException("Path is required for XML import."); }

            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            XmlDocument document = new XmlDocument();
            document.Load(path.FullName);

            XmlNode root = document.DocumentElement;
            if (root == null || !string.Equals(root.LocalName, "Secrets", System.StringComparison.Ordinal))
            {
                throw new InfisicalImportException("XML import expects a root <Secrets> element.");
            }

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node == null || node.NodeType != XmlNodeType.Element) { continue; }
                if (!string.Equals(node.LocalName, "Secret", System.StringComparison.Ordinal)) { continue; }

                string key = ReadChild(node, "SecretName");
                string value = ReadChild(node, "SecretValue");
                if (string.IsNullOrEmpty(key)) { continue; }
                result.Add(new KeyValuePair<string, string>(key, value ?? string.Empty));
            }

            return result;
        }

        private static string ReadChild(XmlNode parent, string name)
        {
            foreach (XmlNode child in parent.ChildNodes)
            {
                if (child == null || child.NodeType != XmlNodeType.Element) { continue; }
                if (string.Equals(child.LocalName, name, System.StringComparison.Ordinal))
                {
                    return child.InnerText;
                }
            }
            return null;
        }
    }
}
