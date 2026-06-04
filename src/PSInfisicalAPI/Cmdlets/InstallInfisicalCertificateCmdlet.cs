using System;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Pki;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Install, "InfisicalCertificate", SupportsShouldProcess = true, DefaultParameterSetName = "FromCertificate")]
    [OutputType(typeof(X509Certificate2))]
    public sealed class InstallInfisicalCertificateCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "FromCertificate", Mandatory = true, ValueFromPipeline = true)]
        public X509Certificate2 Certificate { get; set; }

        [Parameter(ParameterSetName = "FromInfisical", Mandatory = true, ValueFromPipeline = true)]
        public InfisicalCertificate InfisicalCertificate { get; set; }

        [Parameter(ParameterSetName = "FromSerial", Mandatory = true, Position = 0)]
        public string SerialNumber { get; set; }

        [Parameter] public StoreName StoreName { get; set; } = StoreName.My;
        [Parameter] public StoreLocation StoreLocation { get; set; } = StoreLocation.CurrentUser;
        [Parameter] public X509KeyStorageFlags KeyStorageFlags { get; set; } = X509KeyStorageFlags.DefaultKeySet;
        [Parameter] public SwitchParameter IncludeChain { get; set; }
        [Parameter] public SwitchParameter NoPrivateKey { get; set; }
        [Parameter] public SwitchParameter Force { get; set; }
        [Parameter] public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                X509Certificate2 cert = ResolveCertificate();
                if (cert == null)
                {
                    return;
                }

                InstallCertificate(cert, StoreName, StoreLocation);

                if (IncludeChain.IsPresent && string.Equals(ParameterSetName, "FromCertificate", StringComparison.Ordinal) == false)
                {
                    foreach (X509Certificate2 chainCert in ResolveChain())
                    {
                        StoreName chainStore = InfisicalCertificateRequestHelpers.GetChainCertificateTargetStore(chainCert);
                        InstallCertificate(chainCert, chainStore, StoreLocation);
                    }
                }

                if (PassThru.IsPresent)
                {
                    WriteObject(cert);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("InstallInfisicalCertificateCmdlet", "InstallCertificate", exception);
            }
        }

        private void InstallCertificate(X509Certificate2 cert, StoreName storeName, StoreLocation storeLocation)
        {
            string target = string.Concat(storeLocation.ToString(), @"\", storeName.ToString(), " [", cert.Thumbprint, "]");
            X509Store store = new X509Store(storeName, storeLocation);
            try
            {
                store.Open(OpenFlags.ReadWrite);
                X509Certificate2Collection existing = store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);
                if (existing.Count > 0)
                {
                    if (!Force.IsPresent)
                    {
                        Logger.Information("InstallInfisicalCertificateCmdlet", string.Concat("Certificate already present in ", target, "; no action taken."));
                        return;
                    }

                    if (!ShouldProcess(target, "Replace existing certificate"))
                    {
                        return;
                    }

                    store.RemoveRange(existing);
                }
                else if (!ShouldProcess(target, "Install certificate"))
                {
                    return;
                }

                store.Add(cert);
                Logger.Information("InstallInfisicalCertificateCmdlet", string.Concat("Installed certificate to ", target, "."));
            }
            finally
            {
                store.Close();
            }
        }

        private X509Certificate2 ResolveCertificate()
        {
            if (string.Equals(ParameterSetName, "FromCertificate", StringComparison.Ordinal))
            {
                return Certificate;
            }

            string serial = null;
            if (string.Equals(ParameterSetName, "FromSerial", StringComparison.Ordinal))
            {
                serial = SerialNumber;
            }
            else if (InfisicalCertificate != null)
            {
                serial = InfisicalCertificate.SerialNumber;
            }

            if (string.IsNullOrEmpty(serial))
            {
                return null;
            }

            InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
            InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);
            InfisicalCertificateBundle bundle = client.GetCertificateBundle(connection, serial);
            if (bundle == null)
            {
                return null;
            }

            string keyPem = NoPrivateKey.IsPresent ? null : bundle.PrivateKeyPem;
            return PemCertificateBuilder.Build(bundle.CertificatePem, keyPem, bundle.CertificateChainPem, KeyStorageFlags);
        }

        private System.Collections.Generic.List<X509Certificate2> ResolveChain()
        {
            string serial = string.Equals(ParameterSetName, "FromSerial", StringComparison.Ordinal)
                ? SerialNumber
                : (InfisicalCertificate != null ? InfisicalCertificate.SerialNumber : null);
            if (string.IsNullOrEmpty(serial))
            {
                return new System.Collections.Generic.List<X509Certificate2>();
            }

            InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
            InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);
            InfisicalCertificateBundle bundle = client.GetCertificateBundle(connection, serial);
            return bundle != null ? PemCertificateBuilder.ReadCertificateChain(bundle.CertificateChainPem) : new System.Collections.Generic.List<X509Certificate2>();
        }
    }
}
