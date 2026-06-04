using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Pki;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalCertificate")]
    [OutputType(typeof(InfisicalCertificate))]
    public sealed class GetInfisicalCertificateCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [Alias("Id", "Identifier")]
        public string SerialNumber { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);

                InfisicalCertificate cert = client.RetrieveCertificate(connection, SerialNumber);
                if (cert != null)
                {
                    WriteObject(cert);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalCertificateCmdlet", "GetCertificate", exception);
            }
        }
    }
}
