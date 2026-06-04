using System;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Uninstall, "InfisicalCertificate", SupportsShouldProcess = true, DefaultParameterSetName = "ByThumbprint")]
    [OutputType(typeof(X509Certificate2))]
    public sealed class UninstallInfisicalCertificateCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "ByThumbprint", Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Thumbprint { get; set; }

        [Parameter(ParameterSetName = "ByCertificate", Mandatory = true, ValueFromPipeline = true)]
        public X509Certificate2 Certificate { get; set; }

        [Parameter(ParameterSetName = "ByInfisical", Mandatory = true, ValueFromPipeline = true)]
        public InfisicalCertificate InfisicalCertificate { get; set; }

        [Parameter(ParameterSetName = "BySubject", Mandatory = true)]
        public string Subject { get; set; }

        [Parameter] public StoreName StoreName { get; set; } = StoreName.My;
        [Parameter] public StoreLocation StoreLocation { get; set; } = StoreLocation.CurrentUser;
        [Parameter] public SwitchParameter Force { get; set; }
        [Parameter] public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                X509Store store = new X509Store(StoreName, StoreLocation);
                try
                {
                    store.Open(OpenFlags.ReadWrite);
                    X509Certificate2Collection matches = FindMatches(store);
                    string target = string.Concat(StoreLocation.ToString(), @"\", StoreName.ToString());

                    if (matches == null || matches.Count == 0)
                    {
                        Logger.Information("UninstallInfisicalCertificateCmdlet", string.Concat("No matching certificates found in ", target, "; no action taken."));
                        return;
                    }

                    if (matches.Count > 1 && !Force.IsPresent)
                    {
                        throw new InvalidOperationException(string.Concat(
                            "Found ", matches.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                            " matching certificates in ", target, ". Pass -Force to remove all of them."));
                    }

                    foreach (X509Certificate2 match in matches)
                    {
                        string description = string.Concat(target, " [", match.Thumbprint, "]");
                        if (!ShouldProcess(description, "Remove certificate"))
                        {
                            continue;
                        }

                        store.Remove(match);
                        Logger.Information("UninstallInfisicalCertificateCmdlet", string.Concat("Removed certificate from ", description, "."));

                        if (PassThru.IsPresent)
                        {
                            WriteObject(match);
                        }
                    }
                }
                finally
                {
                    store.Close();
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("UninstallInfisicalCertificateCmdlet", "UninstallCertificate", exception);
            }
        }

        private X509Certificate2Collection FindMatches(X509Store store)
        {
            string thumbprint = ResolveThumbprint();
            if (!string.IsNullOrEmpty(thumbprint))
            {
                return store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            }

            if (string.Equals(ParameterSetName, "BySubject", StringComparison.Ordinal))
            {
                return store.Certificates.Find(X509FindType.FindBySubjectName, Subject, false);
            }

            return null;
        }

        private string ResolveThumbprint()
        {
            if (string.Equals(ParameterSetName, "ByThumbprint", StringComparison.Ordinal))
            {
                return Thumbprint;
            }

            if (string.Equals(ParameterSetName, "ByCertificate", StringComparison.Ordinal) && Certificate != null)
            {
                return Certificate.Thumbprint;
            }

            if (string.Equals(ParameterSetName, "ByInfisical", StringComparison.Ordinal) && InfisicalCertificate != null)
            {
                if (!string.IsNullOrEmpty(InfisicalCertificate.FingerprintSha1))
                {
                    return InfisicalCertificate.FingerprintSha1.Replace(":", string.Empty).Replace(" ", string.Empty);
                }
            }

            return null;
        }
    }
}
