using System;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Pki;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsData.ConvertTo, "InfisicalCertificate", DefaultParameterSetName = "FromPipeline")]
    [OutputType(typeof(X509Certificate2))]
    public sealed class ConvertToInfisicalCertificateCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "FromPipeline", Mandatory = true, ValueFromPipeline = true)]
        public InfisicalCertificate Certificate { get; set; }

        [Parameter(ParameterSetName = "FromBundle", Mandatory = true, ValueFromPipeline = true)]
        public InfisicalCertificateBundle Bundle { get; set; }

        [Parameter(ParameterSetName = "FromSerial", Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string SerialNumber { get; set; }

        [Parameter] public SwitchParameter NoPrivateKey { get; set; }

        [Parameter] public X509KeyStorageFlags KeyStorageFlags { get; set; } = X509KeyStorageFlags.DefaultKeySet;

        [Parameter] public SwitchParameter IncludeChain { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalCertificateBundle resolvedBundle = ResolveBundle();
                if (resolvedBundle == null)
                {
                    return;
                }

                string privateKeyPem = NoPrivateKey.IsPresent ? null : resolvedBundle.PrivateKeyPem;
                X509Certificate2 cert = PemCertificateBuilder.Build(resolvedBundle.CertificatePem, privateKeyPem, resolvedBundle.CertificateChainPem, KeyStorageFlags);
                WriteObject(cert);

                if (IncludeChain.IsPresent)
                {
                    foreach (X509Certificate2 chainCert in PemCertificateBuilder.ReadCertificateChain(resolvedBundle.CertificateChainPem))
                    {
                        WriteObject(chainCert);
                    }
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("ConvertToInfisicalCertificateCmdlet", "ConvertToCertificate", exception);
            }
        }

        private InfisicalCertificateBundle ResolveBundle()
        {
            if (string.Equals(ParameterSetName, "FromBundle", StringComparison.Ordinal))
            {
                return Bundle;
            }

            string serial = null;
            if (string.Equals(ParameterSetName, "FromSerial", StringComparison.Ordinal))
            {
                serial = SerialNumber;
            }
            else if (string.Equals(ParameterSetName, "FromPipeline", StringComparison.Ordinal) && Certificate != null)
            {
                serial = Certificate.SerialNumber;
            }

            if (string.IsNullOrEmpty(serial))
            {
                return null;
            }

            InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
            InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);
            return client.GetCertificateBundle(connection, serial);
        }
    }
}
