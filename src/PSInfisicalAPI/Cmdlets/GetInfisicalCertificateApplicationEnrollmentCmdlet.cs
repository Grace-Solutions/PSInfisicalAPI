using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Pki;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalCertificateApplicationEnrollment")]
    [OutputType(typeof(InfisicalCertificateApplicationEnrollment))]
    public sealed class GetInfisicalCertificateApplicationEnrollmentCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [Alias("Id")]
        public string ApplicationId { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        [Alias("CertificateProfileId")]
        public string ProfileId { get; set; }

        [Parameter(Mandatory = true)] public string ProjectId { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);

                InfisicalCertificateApplicationEnrollment enrollment = client.GetCertificateApplicationEnrollment(connection, ApplicationId, ProfileId, ProjectId);
                if (enrollment != null)
                {
                    WriteObject(enrollment);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalCertificateApplicationEnrollmentCmdlet", "GetCertificateApplicationEnrollment", exception);
            }
        }
    }
}
