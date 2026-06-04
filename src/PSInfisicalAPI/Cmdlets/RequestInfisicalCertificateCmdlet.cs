using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Pki;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Request, "InfisicalCertificate", SupportsShouldProcess = true, DefaultParameterSetName = "BySubscriber")]
    [OutputType(typeof(InfisicalCertificateResult))]
    public sealed class RequestInfisicalCertificateCmdlet : InfisicalCmdletBase
    {
        private const string Component = "RequestInfisicalCertificateCmdlet";

        [Parameter(ParameterSetName = "BySubscriber", Mandatory = true, Position = 0)]
        [Alias("Subscriber")]
        public string PkiSubscriberSlug { get; set; }

        [Parameter(ParameterSetName = "ByCa", Mandatory = true, Position = 0)]
        [Alias("CaId")]
        public string CertificateAuthorityId { get; set; }

        [Parameter] public string ProjectId { get; set; }
        [Parameter] public IDictionary Subject { get; set; }
        [Parameter] public string CommonName { get; set; }
        [Parameter] public string Country { get; set; }
        [Parameter] public string State { get; set; }
        [Parameter] public string Locality { get; set; }
        [Parameter] public string Organization { get; set; }
        [Parameter] public string OrganizationalUnit { get; set; }
        [Parameter] public string EmailAddress { get; set; }
        [Parameter] public string[] DnsName { get; set; }
        [Parameter] public string[] IpAddress { get; set; }
        [Parameter] public InfisicalKeyAlgorithm KeyAlgorithm { get; set; } = InfisicalKeyAlgorithm.Rsa;
        [Parameter] public int KeySize { get; set; } = 2048;
        [Parameter] public InfisicalEcCurve Curve { get; set; } = InfisicalEcCurve.P256;

        [Parameter(ParameterSetName = "ByCa")] public string Ttl { get; set; }
        [Parameter(ParameterSetName = "ByCa")] public string NotBefore { get; set; }
        [Parameter(ParameterSetName = "ByCa")] public string NotAfter { get; set; }
        [Parameter(ParameterSetName = "ByCa")] public string FriendlyName { get; set; }
        [Parameter(ParameterSetName = "ByCa")] public string PkiCollectionId { get; set; }
        [Parameter(ParameterSetName = "ByCa")] public string[] KeyUsage { get; set; }
        [Parameter(ParameterSetName = "ByCa")] public string[] ExtendedKeyUsage { get; set; }

        [Parameter] public SwitchParameter Install { get; set; }
        [Parameter] public StoreName StoreName { get; set; } = StoreName.My;
        [Parameter] public StoreLocation StoreLocation { get; set; } = StoreLocation.CurrentUser;
        [Parameter] public X509KeyStorageFlags KeyStorageFlags { get; set; } = X509KeyStorageFlags.DefaultKeySet;
        [Parameter] public SwitchParameter InstallChain { get; set; }

        [Parameter] public InfisicalPrivateKeyProtection PrivateKeyProtection { get; set; } = InfisicalPrivateKeyProtection.LocalOnly;
        [Parameter] public SwitchParameter PersistKey { get; set; }
        [Parameter] public SwitchParameter MachineKey { get; set; }
        [Parameter] public string PrivateKeyPath { get; set; }

        [Parameter] public SwitchParameter AllowRenewal { get; set; }
        [Parameter] public int RenewalThresholdDays { get; set; } = 30;
        [Parameter] public SwitchParameter Force { get; set; }
        [Parameter] public SwitchParameter LocalChainOnly { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);

                InfisicalCsrSubject csrSubject = InfisicalCertificateRequestHelpers.MergeSubject(Subject, CommonName, Country, State, Locality, Organization, OrganizationalUnit, EmailAddress);
                List<string> dnsNames = BuildDnsNames(csrSubject);
                if (string.IsNullOrEmpty(csrSubject.CommonName) && dnsNames.Count > 0) { csrSubject.CommonName = dnsNames[0]; }
                if (string.IsNullOrEmpty(csrSubject.CommonName)) { throw new InvalidOperationException("Subject CommonName could not be determined and no DnsName was provided."); }

                X509Certificate2 existing = TryFindExisting(client, connection, resolvedProjectId, csrSubject.CommonName);
                if (existing != null && !Force.IsPresent && !(AllowRenewal.IsPresent && InfisicalLocalCertificateLookup.IsRenewable(existing, RenewalThresholdDays)))
                {
                    Logger.Information(Component, string.Concat("Reusing existing certificate (Thumbprint=", existing.Thumbprint, ", NotAfter=", existing.NotAfter.ToString("u"), ")."));
                    InfisicalCertificateResult reuseResult = InfisicalCertificateRequestHelpers.BuildResultFromExistingLocal(existing);

                    if (!LocalChainOnly.IsPresent
                        && (reuseResult.Root == null || reuseResult.Intermediates == null || reuseResult.Intermediates.Length == 0)
                        && !string.IsNullOrEmpty(existing.SerialNumber))
                    {
                        try
                        {
                            InfisicalCertificateBundle bundle = client.GetCertificateBundle(connection, existing.SerialNumber);
                            if (bundle != null && !string.IsNullOrEmpty(bundle.CertificateChainPem))
                            {
                                reuseResult = InfisicalCertificateRequestHelpers.BuildResultFromExistingLocal(existing, bundle);
                                Logger.Information(Component, "Reused certificate chain completed from Infisical bundle.");
                            }
                        }
                        catch (Exception bundleException)
                        {
                            Logger.Verbose(Component, string.Concat("Infisical bundle fetch for reuse path failed (continuing with local-only chain): ", bundleException.Message));
                        }
                    }

                    WriteObject(reuseResult);
                    return;
                }

                string target = string.Concat("PKI subscriber '", PkiSubscriberSlug ?? "(n/a)", "' or CA '", CertificateAuthorityId ?? "(n/a)", "' for CN=", csrSubject.CommonName);
                if (!ShouldProcess(target, "Request new certificate")) { return; }

                InfisicalCsrOptions csrOptions = new InfisicalCsrOptions { KeyAlgorithm = KeyAlgorithm, RsaKeySize = KeySize, EcCurve = Curve };
                InfisicalCsrResult csr = InfisicalCsrBuilder.Build(csrSubject, dnsNames, IpAddress, csrOptions);
                InfisicalSignedCertificate signed = SignCertificate(client, connection, resolvedProjectId, csr.CsrPem);
                signed.PrivateKeyPem = csr.PrivateKeyPem;

                X509KeyStorageFlags resolvedFlags = ResolveEffectiveKeyStorageFlags();
                X509Certificate2 cert = PemCertificateBuilder.Build(signed.CertificatePem, signed.PrivateKeyPem, signed.CertificateChainPem, resolvedFlags);

                if (Install.IsPresent)
                {
                    InfisicalCertificateRequestHelpers.InstallToStore(cert, StoreName, StoreLocation, Force.IsPresent, Logger, Component);
                    if (InstallChain.IsPresent)
                    {
                        InfisicalCertificateRequestHelpers.InstallChain(signed, StoreLocation, Force.IsPresent, Logger, Component);
                    }
                }

                InfisicalCertificateResult resultObj = InfisicalCertificateRequestHelpers.BuildResult(cert, signed);

                bool hasExplicitPath = !string.IsNullOrEmpty(PrivateKeyPath);
                if (hasExplicitPath && !string.IsNullOrEmpty(resultObj.PrivateKeyPem))
                {
                    InfisicalCertificateRequestHelpers.WritePrivateKeyPem(resultObj.PrivateKeyPem, PrivateKeyPath);
                    Logger.Information(Component, string.Concat("Wrote private key PEM to '", PrivateKeyPath, "'."));
                }

                if (!MyInvocation.BoundParameters.ContainsKey("KeyStorageFlags")
                    && InfisicalCertificateRequestHelpers.ShouldScrubPrivateKeyPem(PrivateKeyProtection, hasExplicitPath))
                {
                    resultObj.PrivateKeyPem = null;
                }

                WriteObject(resultObj);
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException(Component, "RequestCertificate", exception);
            }
        }

        private List<string> BuildDnsNames(InfisicalCsrSubject subject)
        {
            List<string> result = new List<string>();
            if (DnsName != null) { foreach (string dns in DnsName) { if (!string.IsNullOrEmpty(dns)) { result.Add(dns); } } }
            if (result.Count == 0)
            {
                string fqdn = InfisicalCertificateRequestHelpers.ResolveLocalFqdn();
                if (!string.IsNullOrEmpty(fqdn)) { result.Add(fqdn); }
            }

            if (!string.IsNullOrEmpty(subject.CommonName) && !result.Contains(subject.CommonName)) { result.Insert(0, subject.CommonName); }
            return result;
        }

        private X509Certificate2 TryFindExisting(InfisicalPkiClient client, InfisicalConnection connection, string projectId, string commonName)
        {
            List<string> candidateSerials = new List<string>();
            try
            {
                InfisicalCertificateSearchQuery query = new InfisicalCertificateSearchQuery { ProjectId = projectId, CommonName = commonName, Status = "active", Limit = 50 };
                InfisicalCertificateSearchResult page = client.SearchCertificates(connection, query);
                if (page != null && page.Certificates != null)
                {
                    foreach (InfisicalCertificate hit in page.Certificates) { if (!string.IsNullOrEmpty(hit.SerialNumber)) { candidateSerials.Add(hit.SerialNumber); } }
                }
            }
            catch (Exception searchException)
            {
                Logger.Verbose(Component, string.Concat("Infisical search for idempotency check failed: ", searchException.Message));
            }

            return InfisicalLocalCertificateLookup.FindMatch(StoreName, StoreLocation, commonName, candidateSerials);
        }

        private X509KeyStorageFlags ResolveEffectiveKeyStorageFlags()
        {
            if (MyInvocation.BoundParameters.ContainsKey("KeyStorageFlags"))
            {
                return KeyStorageFlags;
            }

            return InfisicalCertificateRequestHelpers.ResolveKeyStorageFlags(PrivateKeyProtection, PersistKey.IsPresent, MachineKey.IsPresent);
        }

        private InfisicalSignedCertificate SignCertificate(InfisicalPkiClient client, InfisicalConnection connection, string projectId, string csrPem)
        {
            if (string.Equals(ParameterSetName, "BySubscriber", StringComparison.Ordinal))
            {
                return client.SignCertificateBySubscriber(connection, PkiSubscriberSlug, projectId, csrPem);
            }

            return client.SignCertificateByCa(connection, CertificateAuthorityId, csrPem, CommonName, null, Ttl, NotBefore, NotAfter, FriendlyName, PkiCollectionId, KeyUsage, ExtendedKeyUsage);
        }
    }
}
