using System.IO;
using System.Text;
using System.Xml;
using PSInfisicalAPI.Common;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Exports
{
    public sealed class XmlInfisicalExporter : IInfisicalExporter
    {
        public void Export(InfisicalExportRequest request)
        {
            if (request == null || request.Secrets == null) { throw new InfisicalExportException("Export request is invalid."); }
            if (request.Path == null) { throw new InfisicalExportException("Path is required for XML export."); }

            InfisicalPath.EnsureParentDirectoryExists(request.Path);

            Encoding encoding = request.Encoding ?? new UTF8Encoding(false);

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = encoding,
                OmitXmlDeclaration = false
            };

            using (FileStream stream = new FileStream(request.Path.FullName, FileMode.Create, FileAccess.Write))
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Secrets");

                foreach (InfisicalSecret secret in request.Secrets)
                {
                    if (secret == null) { continue; }

                    secret.UsePlainTextValue(plainValue =>
                    {
                        writer.WriteStartElement("Secret");
                        writer.WriteElementString("SecretName", secret.SecretName ?? string.Empty);
                        writer.WriteElementString("SecretValue", plainValue ?? string.Empty);
                        writer.WriteElementString("SecretPath", secret.SecretPath ?? string.Empty);
                        writer.WriteStartElement("SecretMetadata");

                        if (secret.SecretMetadata != null)
                        {
                            foreach (InfisicalSecretMetadata entry in secret.SecretMetadata)
                            {
                                if (entry == null || string.IsNullOrEmpty(entry.Key)) { continue; }
                                writer.WriteStartElement("Metadata");
                                writer.WriteElementString("Key", entry.Key);
                                writer.WriteElementString("Value", entry.Value ?? string.Empty);
                                writer.WriteEndElement();
                            }
                        }

                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    });
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
