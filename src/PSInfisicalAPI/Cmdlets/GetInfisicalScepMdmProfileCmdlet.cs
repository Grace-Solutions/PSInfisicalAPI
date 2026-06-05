using System;
using System.Globalization;
using System.Management.Automation;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Pki;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalScepMdmProfile", DefaultParameterSetName = "FromEnrollment")]
    [OutputType(typeof(InfisicalScepMdmProfile))]
    public sealed class GetInfisicalScepMdmProfileCmdlet : InfisicalCmdletBase
    {
        private const string Component = "GetInfisicalScepMdmProfileCmdlet";

        [Parameter(ParameterSetName = "FromEnrollment", Mandatory = true, ValueFromPipeline = true, Position = 0)]
        [Alias("Enrollment")]
        public InfisicalCertificateApplicationEnrollment EnrollmentObject { get; set; }

        [Parameter(ParameterSetName = "FromProfile", Mandatory = true, ValueFromPipeline = true, Position = 0)]
        [Alias("Profile", "CertificateProfile")]
        public InfisicalCertificateProfile InputObject { get; set; }

        [Parameter(ParameterSetName = "FromProfile", Mandatory = true)]
        [Alias("AppId")]
        public string ApplicationId { get; set; }

        [Parameter(ParameterSetName = "FromEnrollment")]
        [Parameter(ParameterSetName = "FromProfile")]
        [Parameter(ParameterSetName = "Manual", Mandatory = true)]
        public SecureString Challenge { get; set; }

        [Parameter(ParameterSetName = "Manual", Mandatory = true)]
        [Parameter(ParameterSetName = "FromProfile")]
        [Parameter(ParameterSetName = "FromEnrollment")]
        public string ServerUrl { get; set; }

        [Parameter] public string UniqueId { get; set; }

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
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();

                if (string.Equals(ParameterSetName, "FromEnrollment", StringComparison.Ordinal))
                {
                    WriteObject(BuildFromEnrollment(connection));
                    return;
                }

                if (string.Equals(ParameterSetName, "FromProfile", StringComparison.Ordinal))
                {
                    WriteObject(BuildFromProfile(connection));
                    return;
                }

                WriteObject(BuildManual(connection));
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException(Component, "GetScepMdmProfile", exception);
            }
        }

        private InfisicalScepMdmProfile BuildFromEnrollment(InfisicalConnection connection)
        {
            if (EnrollmentObject == null) { throw new InvalidOperationException("EnrollmentObject is required."); }
            if (string.IsNullOrEmpty(EnrollmentObject.ApplicationId)) { throw new InvalidOperationException("EnrollmentObject.ApplicationId is required."); }
            if (string.IsNullOrEmpty(EnrollmentObject.ProfileId)) { throw new InvalidOperationException("EnrollmentObject.ProfileId is required."); }

            InfisicalCertificateApplicationScepEnrollment scep = EnrollmentObject.Scep;
            if (scep == null) { throw new InvalidOperationException("Enrollment does not have SCEP configured."); }

            string resolvedServerUrl = FirstNonEmpty(ServerUrl, scep.ScepEndpointUrl, BuildDefaultServerUrl(connection, EnrollmentObject.ApplicationId, EnrollmentObject.ProfileId));
            string resolvedUniqueId = !string.IsNullOrEmpty(UniqueId) ? UniqueId : SanitizeForCspId(EnrollmentObject.ProfileId);
            string resolvedThumbprint = !string.IsNullOrEmpty(CAThumbprint) ? CAThumbprint : scep.RaCertificateThumbprint;
            string resolvedChallenge = ResolveChallengeFromEnrollment(connection, scep);

            InfisicalScepMdmProfile result = NewProfileShell(resolvedUniqueId, resolvedServerUrl, resolvedChallenge, resolvedThumbprint, null, null);
            result.SourceProfileId = EnrollmentObject.ProfileId;
            Logger.Verbose(Component, string.Concat("Built SCEP MDM profile from enrollment for application '", EnrollmentObject.ApplicationId, "' / profile '", EnrollmentObject.ProfileId, "' targeting ", result.ServerUrl, " (UniqueId=", result.UniqueId, ", Scope=", result.Scope, ", ChallengeType=", scep.ChallengeType ?? "<unknown>", ")."));
            return result;
        }

        private InfisicalScepMdmProfile BuildFromProfile(InfisicalConnection connection)
        {
            if (InputObject == null) { throw new InvalidOperationException("InputObject is required."); }
            if (string.IsNullOrEmpty(InputObject.Id)) { throw new InvalidOperationException("InputObject.Id is required."); }
            if (string.IsNullOrEmpty(ApplicationId)) { throw new InvalidOperationException("ApplicationId is required when binding by certificate profile."); }
            if (Challenge == null) { throw new InvalidOperationException("Challenge is required when building from a certificate profile."); }

            string resolvedServerUrl = !string.IsNullOrEmpty(ServerUrl) ? ServerUrl : BuildDefaultServerUrl(connection, ApplicationId, InputObject.Id);
            string resolvedUniqueId = !string.IsNullOrEmpty(UniqueId) ? UniqueId : SanitizeForCspId(!string.IsNullOrEmpty(InputObject.Slug) ? InputObject.Slug : InputObject.Id);
            InfisicalCertificateProfileDefaults defaults = InputObject.Defaults;
            string resolvedKeyAlgorithm = !string.IsNullOrEmpty(KeyAlgorithm) ? KeyAlgorithm : MapKeyAlgorithm(defaults != null ? defaults.KeyAlgorithm : null);
            string resolvedEku = !string.IsNullOrEmpty(EkuMapping) ? EkuMapping : JoinEkuOids(defaults != null ? defaults.ExtendedKeyUsages : null);

            InfisicalScepMdmProfile result = NewProfileShell(resolvedUniqueId, resolvedServerUrl, SecureStringToPlainText(Challenge), CAThumbprint, resolvedKeyAlgorithm, resolvedEku);
            result.SourceProfileId = InputObject.Id;
            result.SourceProfileSlug = InputObject.Slug;
            Logger.Verbose(Component, string.Concat("Built SCEP MDM profile for source profile '", InputObject.Slug ?? InputObject.Id, "' targeting ", result.ServerUrl, " (UniqueId=", result.UniqueId, ", Scope=", result.Scope, ")."));
            return result;
        }

        private InfisicalScepMdmProfile BuildManual(InfisicalConnection connection)
        {
            if (string.IsNullOrEmpty(UniqueId)) { throw new InvalidOperationException("UniqueId is required in Manual mode."); }
            string resolvedChallenge = SecureStringToPlainText(Challenge);
            InfisicalScepMdmProfile result = NewProfileShell(UniqueId, ServerUrl, resolvedChallenge, CAThumbprint, KeyAlgorithm, EkuMapping);
            Logger.Verbose(Component, string.Concat("Built SCEP MDM profile in Manual mode targeting ", result.ServerUrl, " (UniqueId=", result.UniqueId, ", Scope=", result.Scope, ")."));
            return result;
        }

        private InfisicalScepMdmProfile NewProfileShell(string uniqueId, string serverUrl, string challenge, string thumbprint, string keyAlgorithm, string ekuMapping)
        {
            return new InfisicalScepMdmProfile
            {
                UniqueId = uniqueId,
                Scope = Scope,
                ServerUrl = serverUrl,
                Challenge = challenge,
                SubjectName = SubjectName,
                SubjectAlternativeNames = SubjectAlternativeNames,
                EkuMapping = ekuMapping,
                KeyUsage = KeyUsage,
                KeyLength = KeyLength,
                KeyAlgorithm = keyAlgorithm,
                HashAlgorithm = HashAlgorithm,
                KeyProtection = KeyProtection,
                ContainerName = ContainerName,
                ValidPeriod = ValidPeriod,
                ValidPeriodUnits = ValidPeriodUnits,
                RetryCount = RetryCount,
                RetryDelay = RetryDelay,
                TemplateName = TemplateName,
                CAThumbprint = thumbprint,
                CustomTextToShowInPrompt = CustomTextToShowInPrompt
            };
        }

        private string ResolveChallengeFromEnrollment(InfisicalConnection connection, InfisicalCertificateApplicationScepEnrollment scep)
        {
            if (Challenge != null) { return SecureStringToPlainText(Challenge); }

            string challengeType = scep.ChallengeType ?? string.Empty;
            if (string.Equals(challengeType, "dynamic", StringComparison.OrdinalIgnoreCase))
            {
                InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);
                Logger.Verbose(Component, "Minting SCEP dynamic challenge for enrollment.");
                return client.GenerateScepDynamicChallenge(connection, EnrollmentObject.ApplicationId, EnrollmentObject.ProfileId);
            }

            throw new InvalidOperationException(string.Concat("Enrollment uses challengeType '", challengeType, "'. Supply -Challenge with the configured static challenge password."));
        }

        private static string BuildDefaultServerUrl(InfisicalConnection connection, string applicationId, string profileId)
        {
            if (connection == null || connection.BaseUri == null) { throw new InvalidOperationException("Active Infisical connection is required to derive ServerUrl."); }
            string baseUrl = connection.BaseUri.GetLeftPart(UriPartial.Authority);
            return string.Concat(baseUrl, "/scep/applications/", applicationId, "/profiles/", profileId, "/pkiclient.exe");
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null) { return null; }
            foreach (string value in values) { if (!string.IsNullOrEmpty(value)) { return value; } }
            return null;
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
