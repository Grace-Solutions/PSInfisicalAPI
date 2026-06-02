using System.IO;
using System.Text;
using PSInfisicalAPI.Common;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Exports
{
    public sealed class EnvInfisicalExporter : IInfisicalExporter
    {
        public void Export(InfisicalExportRequest request)
        {
            if (request == null || request.Secrets == null) { throw new InfisicalExportException("Export request is invalid."); }
            if (request.Path == null) { throw new InfisicalExportException("Path is required for ENV export."); }

            InfisicalPath.EnsureParentDirectoryExists(request.Path);

            StringBuilder builder = new StringBuilder();

            foreach (InfisicalSecret secret in request.Secrets)
            {
                if (secret == null || string.IsNullOrEmpty(secret.SecretName)) { continue; }

                secret.UsePlainTextValue(plainValue =>
                {
                    builder.Append(secret.SecretName);
                    builder.Append('=');
                    builder.AppendLine(plainValue ?? string.Empty);
                });
            }

            Encoding encoding = request.Encoding ?? new UTF8Encoding(false);
            File.WriteAllText(request.Path.FullName, builder.ToString(), encoding);
        }
    }
}
