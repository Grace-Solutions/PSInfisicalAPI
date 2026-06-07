using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text;
using PSInfisicalAPI.Common;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Exports;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    public enum InfisicalExportEncoding
    {
        UTF8,
        UTF8Bom,
        Unicode
    }

    [Cmdlet(VerbsData.Export, "InfisicalSecrets")]
    public sealed class ExportInfisicalSecretsCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public InfisicalSecret[] InputObject { get; set; }

        [Parameter(Mandatory = true)]
        public InfisicalExportFormat Format { get; set; }

        [Parameter]
        public FileInfo Path { get; set; }

        [Parameter]
        public EnvironmentVariableTarget Scope { get; set; } = EnvironmentVariableTarget.Process;

        [Parameter]
        public SwitchParameter Force { get; set; }

        [Parameter]
        public InfisicalExportEncoding Encoding { get; set; } = InfisicalExportEncoding.UTF8;

        [Parameter]
        public string Prefix { get; set; }

        [Parameter]
        public SwitchParameter ForcePrefix { get; set; }

        private readonly List<InfisicalSecret> _buffer = new List<InfisicalSecret>();

        protected override void ProcessRecord()
        {
            if (InputObject == null) { return; }

            foreach (InfisicalSecret secret in InputObject)
            {
                if (secret != null)
                {
                    _buffer.Add(secret);
                }
            }
        }

        protected override void EndProcessing()
        {
            try
            {
                bool requiresPath = Format != InfisicalExportFormat.EnvironmentVariables;
                if (requiresPath && Path == null)
                {
                    throw new InfisicalExportException(string.Concat("Path is required for format ", Format.ToString(), "."));
                }

                if (requiresPath && Path.Exists && !Force.IsPresent)
                {
                }

                InfisicalExportRequest request = new InfisicalExportRequest
                {
                    Secrets = ApplyPrefix(_buffer, Prefix, ForcePrefix.IsPresent),
                    Format = Format,
                    Path = Path,
                    Scope = Scope,
                    Force = Force.IsPresent,
                    Encoding = ResolveEncoding(Encoding)
                };

                IInfisicalExporter exporter = InfisicalExporterFactory.Create(Format);
                exporter.Export(request);
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("ExportInfisicalSecretsCmdlet", string.Concat("Export-", Format.ToString()), exception);
            }
        }

        private static InfisicalSecret[] ApplyPrefix(List<InfisicalSecret> source, string prefix, bool force)
        {
            if (string.IsNullOrEmpty(prefix)) { return source.ToArray(); }

            InfisicalSecret[] result = new InfisicalSecret[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                InfisicalSecret original = source[i];
                result[i] = new InfisicalSecret
                {
                    Id = original.Id,
                    InternalId = original.InternalId,
                    Workspace = original.Workspace,
                    Environment = original.Environment,
                    Version = original.Version,
                    Type = original.Type,
                    SecretName = InfisicalPrefix.Apply(original.SecretName, prefix, force),
                    SecretValue = original.SecretValue,
                    SecretValueHidden = original.SecretValueHidden,
                    SecretPath = original.SecretPath,
                    SecretComment = original.SecretComment,
                    CreatedAtUtc = original.CreatedAtUtc,
                    UpdatedAtUtc = original.UpdatedAtUtc,
                    IsRotatedSecret = original.IsRotatedSecret,
                    RotationId = original.RotationId,
                    Tags = original.Tags,
                    SecretMetadata = original.SecretMetadata
                };
            }
            return result;
        }

        private static Encoding ResolveEncoding(InfisicalExportEncoding encoding)
        {
            switch (encoding)
            {
                case InfisicalExportEncoding.UTF8: return new UTF8Encoding(false);
                case InfisicalExportEncoding.UTF8Bom: return new UTF8Encoding(true);
                case InfisicalExportEncoding.Unicode: return new UnicodeEncoding();
                default: return new UTF8Encoding(false);
            }
        }
    }
}
