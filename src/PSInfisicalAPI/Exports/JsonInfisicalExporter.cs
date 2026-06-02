using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using PSInfisicalAPI.Common;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Exports
{
    public sealed class JsonInfisicalExporter : IInfisicalExporter
    {
        public void Export(InfisicalExportRequest request)
        {
            if (request == null || request.Secrets == null) { throw new InfisicalExportException("Export request is invalid."); }
            if (request.Path == null) { throw new InfisicalExportException("Path is required for JSON export."); }

            InfisicalPath.EnsureParentDirectoryExists(request.Path);

            List<Dictionary<string, object>> payload = new List<Dictionary<string, object>>();

            foreach (InfisicalSecret secret in request.Secrets)
            {
                if (secret == null) { continue; }

                secret.UsePlainTextValue(plainValue =>
                {
                    Dictionary<string, object> entry = new Dictionary<string, object>();
                    entry["SecretName"] = secret.SecretName;
                    entry["SecretValue"] = plainValue;
                    entry["SecretPath"] = secret.SecretPath;
                    entry["SecretMetadata"] = MetadataToObject(secret);
                    payload.Add(entry);
                });
            }

            string serialized = JsonConvert.SerializeObject(payload, Formatting.Indented);

            Encoding encoding = request.Encoding ?? new UTF8Encoding(false);
            File.WriteAllText(request.Path.FullName, serialized, encoding);
        }

        private static Dictionary<string, string> MetadataToObject(InfisicalSecret secret)
        {
            Dictionary<string, string> metadata = new Dictionary<string, string>();

            if (secret.SecretMetadata == null) { return metadata; }

            foreach (InfisicalSecretMetadata entry in secret.SecretMetadata)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Key)) { continue; }
                metadata[entry.Key] = entry.Value;
            }

            return metadata;
        }
    }
}
