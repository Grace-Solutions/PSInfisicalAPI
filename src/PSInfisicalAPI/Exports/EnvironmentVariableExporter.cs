using System;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Exports
{
    public sealed class EnvironmentVariableExporter : IInfisicalExporter
    {
        public void Export(InfisicalExportRequest request)
        {
            if (request == null || request.Secrets == null) { throw new InfisicalExportException("Export request is invalid."); }

            EnvironmentVariableTarget target = request.Scope;

            foreach (InfisicalSecret secret in request.Secrets)
            {
                if (secret == null || string.IsNullOrEmpty(secret.SecretName)) { continue; }

                string name = secret.SecretName;

                secret.UsePlainTextValue(plainValue =>
                {
                    Environment.SetEnvironmentVariable(name, plainValue, target);
                });
            }
        }
    }
}
