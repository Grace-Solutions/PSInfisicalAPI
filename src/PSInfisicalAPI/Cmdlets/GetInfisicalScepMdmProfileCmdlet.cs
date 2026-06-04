using System;
using System.Globalization;
using System.Management.Automation;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalScepMdmProfile")]
    [OutputType(typeof(InfisicalScepMdmProfile))]
    public sealed class GetInfisicalScepMdmProfileCmdlet : InfisicalCmdletBase
    {
        private const string Component = "GetInfisicalScepMdmProfileCmdlet";

        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        [Alias("Profile", "CertificateProfile")]
        public InfisicalCertificateProfile InputObject { get; set; }

        [Parameter(Mandatory = true)]
        public SecureString Challenge { get; set; }

        [Parameter] public string UniqueId { get; set; }
        [Parameter] public string ServerUrl { get; set; }

        [Parameter]
        [ValidateSet("Device", "User")]
        public string Scope { get; set; } = "Device";

        [Parameter] public string SubjectName { get; set; }
        [Parameter] public string SubjectAlternativeNames { get; set; }
        [Parameter] public string EkuMapping { get; set; }
        [Parameter] public int? KeyUsage { get; set; }

        [Parameter] public int? KeyLength { get; set; }
        [Parameter] public string KeyAlgorithm { get; set; }
        [Parameter] public string HashAlgorithm { get; set; }
        [Parameter] public int? KeyProtection { get; set; }
        [Parameter] public string ContainerName { get; set; }

        [Parameter] public string ValidPeriod { get; set; }
        [Parameter] public int? ValidPeriodUnits { get; set; }
        [Parameter] public int? RetryCount { get; set; }
        [Parameter] public int? RetryDelay { get; set; }

        [Parameter] public string TemplateName { get; set; }
        [Parameter] public string CAThumbprint { get; set; }
        [Parameter] public string CustomTextToShowInPrompt { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (InputObject == null) { throw new InvalidOperationException("InputObject is required."); }
                if (string.IsNullOrEmpty(InputObject.Id)) { throw new InvalidOperationException("InputObject.Id is required."); }

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedServerUrl = !string.IsNullOrEmpty(ServerUrl) ? ServerUrl : BuildDefaultServerUrl(connection, InputObject.Id);
                string resolvedUniqueId = !string.IsNullOrEmpty(UniqueId) ? UniqueId : SanitizeForCspId(!string.IsNullOrEmpty(InputObject.Slug) ? InputObject.Slug : InputObject.Id);

                InfisicalCertificateProfileDefaults defaults = InputObject.Defaults;
                string resolvedKeyAlgorithm = !string.IsNullOrEmpty(KeyAlgorithm) ? KeyAlgorithm : MapKeyAlgorithm(defaults != null ? defaults.KeyAlgorithm : null);
                string resolvedEku = !string.IsNullOrEmpty(EkuMapping) ? EkuMapping : JoinEkuOids(defaults != null ? defaults.ExtendedKeyUsages : null);

                InfisicalScepMdmProfile result = new InfisicalScepMdmProfile
                {
                    UniqueId = resolvedUniqueId,
                    Scope = Scope,
                    ServerUrl = resolvedServerUrl,
                    Challenge = SecureStringToPlainText(Challenge),
                    SubjectName = SubjectName,
                    SubjectAlternativeNames = SubjectAlternativeNames,
                    EkuMapping = resolvedEku,
                    KeyUsage = KeyUsage,
                    KeyLength = KeyLength,
                    KeyAlgorithm = resolvedKeyAlgorithm,
                    HashAlgorithm = HashAlgorithm,
                    KeyProtection = KeyProtection,
                    ContainerName = ContainerName,
                    ValidPeriod = ValidPeriod,
                    ValidPeriodUnits = ValidPeriodUnits,
                    RetryCount = RetryCount,
                    RetryDelay = RetryDelay,
                    TemplateName = TemplateName,
                    CAThumbprint = CAThumbprint,
                    CustomTextToShowInPrompt = CustomTextToShowInPrompt,
                    SourceProfileId = InputObject.Id,
                    SourceProfileSlug = InputObject.Slug
                };

                Logger.Verbose(Component, string.Concat("Built SCEP MDM profile for source profile '", InputObject.Slug ?? InputObject.Id, "' targeting ", result.ServerUrl, " (UniqueId=", result.UniqueId, ", Scope=", result.Scope, ")."));
                WriteObject(result);
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException(Component, "GetScepMdmProfile", exception);
            }
        }

        private static string BuildDefaultServerUrl(InfisicalConnection connection, string profileId)
        {
            if (connection == null || connection.BaseUri == null) { throw new InvalidOperationException("Active Infisical connection is required to derive ServerUrl."); }
            string baseUrl = connection.BaseUri.GetLeftPart(UriPartial.Authority);
            return string.Concat(baseUrl, "/scep/", profileId, "/pkiclient.exe");
        }

        private static string SanitizeForCspId(string input)
        {
            if (string.IsNullOrEmpty(input)) { return "Infisical"; }
            char[] buffer = new char[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                buffer[i] = (char.IsLetterOrDigit(c) || c == '-' || c == '_') ? c : '_';
            }
            return new string(buffer);
        }

        private static string MapKeyAlgorithm(string fromDefaults)
        {
            if (string.IsNullOrEmpty(fromDefaults)) { return null; }
            if (fromDefaults.IndexOf("rsa", StringComparison.OrdinalIgnoreCase) >= 0) { return "RSA"; }
            if (fromDefaults.IndexOf("ec", StringComparison.OrdinalIgnoreCase) >= 0) { return "ECDSA_P256"; }
            return null;
        }

        private static string JoinEkuOids(string[] values)
        {
            if (values == null || values.Length == 0) { return null; }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            bool first = true;
            foreach (string v in values)
            {
                if (string.IsNullOrEmpty(v)) { continue; }
                if (!first) { sb.Append('+'); }
                sb.Append(v);
                first = false;
            }
            return sb.Length > 0 ? sb.ToString() : null;
        }

        private static string SecureStringToPlainText(SecureString value)
        {
            if (value == null) { return null; }
            IntPtr ptr = Marshal.SecureStringToGlobalAllocUnicode(value);
            try { return Marshal.PtrToStringUni(ptr); }
            finally { if (ptr != IntPtr.Zero) { Marshal.ZeroFreeGlobalAllocUnicode(ptr); } }
        }
    }
}
