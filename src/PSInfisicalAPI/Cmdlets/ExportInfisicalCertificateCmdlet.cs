using System;
using System.IO;
using System.Management.Automation;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Pki;
using PSInfisicalAPI.Security;

namespace PSInfisicalAPI.Cmdlets
{
    public enum InfisicalCertificateExportFormat
    {
        Pem,
        Pfx,
        Cer
    }

    [Cmdlet(VerbsData.Export, "InfisicalCertificate", SupportsShouldProcess = true, DefaultParameterSetName = "FromCertificate")]
    [OutputType(typeof(FileInfo))]
    public sealed class ExportInfisicalCertificateCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "FromCertificate", Mandatory = true, ValueFromPipeline = true)]
        public X509Certificate2 Certificate { get; set; }

        [Parameter(ParameterSetName = "FromBundle", Mandatory = true, ValueFromPipeline = true)]
        public InfisicalCertificateBundle Bundle { get; set; }

        [Parameter(ParameterSetName = "FromInfisical", Mandatory = true, ValueFromPipeline = true)]
        public InfisicalCertificate InfisicalCertificate { get; set; }

        [Parameter(ParameterSetName = "FromSerial", Mandatory = true, Position = 1)]
        public string SerialNumber { get; set; }

        [Parameter(Mandatory = true, Position = 0)] public string Path { get; set; }
        [Parameter(Mandatory = true)] public InfisicalCertificateExportFormat Format { get; set; }
        [Parameter] public SecureString Password { get; set; }
        [Parameter] public SwitchParameter IncludeChain { get; set; }
        [Parameter] public SwitchParameter NoPrivateKey { get; set; }
        [Parameter] public SwitchParameter Force { get; set; }
        [Parameter] public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                string resolvedPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(Path);
                if (File.Exists(resolvedPath) && !Force.IsPresent)
                {
                    throw new IOException(string.Concat("File '", resolvedPath, "' already exists. Pass -Force to overwrite."));
                }

                if (!ShouldProcess(resolvedPath, string.Concat("Export certificate as ", Format.ToString())))
                {
                    return;
                }

                InfisicalCertificateBundle bundle = null;
                X509Certificate2 cert = ResolveCertificate(out bundle);
                if (cert == null && bundle == null)
                {
                    return;
                }

                switch (Format)
                {
                    case InfisicalCertificateExportFormat.Pem:
                        WritePem(resolvedPath, cert, bundle);
                        break;
                    case InfisicalCertificateExportFormat.Pfx:
                        WritePfx(resolvedPath, cert);
                        break;
                    case InfisicalCertificateExportFormat.Cer:
                        WriteCer(resolvedPath, cert);
                        break;
                }

                Logger.Information("ExportInfisicalCertificateCmdlet", string.Concat("Exported certificate to '", resolvedPath, "'."));
                if (PassThru.IsPresent)
                {
                    WriteObject(new FileInfo(resolvedPath));
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("ExportInfisicalCertificateCmdlet", "ExportCertificate", exception);
            }
        }

        private X509Certificate2 ResolveCertificate(out InfisicalCertificateBundle bundle)
        {
            bundle = null;
            if (string.Equals(ParameterSetName, "FromCertificate", StringComparison.Ordinal))
            {
                return Certificate;
            }

            if (string.Equals(ParameterSetName, "FromBundle", StringComparison.Ordinal))
            {
                bundle = Bundle;
            }
            else
            {
                string serial = string.Equals(ParameterSetName, "FromSerial", StringComparison.Ordinal)
                    ? SerialNumber
                    : (InfisicalCertificate != null ? InfisicalCertificate.SerialNumber : null);
                if (string.IsNullOrEmpty(serial)) { return null; }

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);
                bundle = client.GetCertificateBundle(connection, serial);
            }

            if (bundle == null) { return null; }
            string keyPem = NoPrivateKey.IsPresent ? null : bundle.PrivateKeyPem;
            return PemCertificateBuilder.Build(bundle.CertificatePem, keyPem, bundle.CertificateChainPem, X509KeyStorageFlags.Exportable);
        }

        private void WritePem(string path, X509Certificate2 cert, InfisicalCertificateBundle bundle)
        {
            StringBuilder sb = new StringBuilder();
            if (bundle != null && !string.IsNullOrEmpty(bundle.CertificatePem))
            {
                sb.Append(bundle.CertificatePem.TrimEnd()).Append('\n');
                if (IncludeChain.IsPresent && !string.IsNullOrEmpty(bundle.CertificateChainPem))
                {
                    sb.Append(bundle.CertificateChainPem.TrimEnd()).Append('\n');
                }

                if (!NoPrivateKey.IsPresent && !string.IsNullOrEmpty(bundle.PrivateKeyPem))
                {
                    sb.Append(bundle.PrivateKeyPem.TrimEnd()).Append('\n');
                }
            }
            else
            {
                sb.Append("-----BEGIN CERTIFICATE-----\n");
                sb.Append(Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
                sb.Append("\n-----END CERTIFICATE-----\n");
            }

            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        }

        private void WritePfx(string path, X509Certificate2 cert)
        {
            byte[] bytes = Password != null
                ? SecureStringUtility.UsePlainText(Password, plain => cert.Export(X509ContentType.Pfx, plain ?? string.Empty))
                : cert.Export(X509ContentType.Pfx);

            File.WriteAllBytes(path, bytes);
        }

        private void WriteCer(string path, X509Certificate2 cert)
        {
            File.WriteAllBytes(path, cert.Export(X509ContentType.Cert));
        }
    }
}
