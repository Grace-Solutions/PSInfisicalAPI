using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Pki;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalCertificateProfile", DefaultParameterSetName = "List")]
    [OutputType(typeof(InfisicalCertificateProfile))]
    public sealed class GetInfisicalCertificateProfileCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "ById", Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [Alias("Id", "CertificateProfileId")]
        public string ProfileId { get; set; }

        [Parameter(Mandatory = true)] public string ProjectId { get; set; }

        [Parameter(ParameterSetName = "List")] public int? Limit { get; set; }

        [Parameter(ParameterSetName = "List")] public int? Offset { get; set; }

        [Parameter(ParameterSetName = "List")] public SwitchParameter IncludeConfigs { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);

                if (string.Equals(ParameterSetName, "ById", StringComparison.Ordinal))
                {
                    InfisicalCertificateProfile profile = client.GetCertificateProfile(connection, ProfileId, ProjectId);
                    if (profile != null)
                    {
                        WriteObject(profile);
                    }

                    return;
                }

                bool? includeConfigs = MyInvocation.BoundParameters.ContainsKey("IncludeConfigs") ? (bool?)IncludeConfigs.IsPresent : null;
                InfisicalCertificateProfile[] all = client.ListCertificateProfiles(connection, ProjectId, Limit, Offset, includeConfigs);
                foreach (InfisicalCertificateProfile profile in all)
                {
                    WriteObject(profile);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalCertificateProfileCmdlet", "GetCertificateProfile", exception);
            }
        }
    }
}
