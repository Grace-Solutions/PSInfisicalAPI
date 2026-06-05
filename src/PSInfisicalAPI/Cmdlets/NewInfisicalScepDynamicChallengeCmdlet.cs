using System;
using System.Management.Automation;
using System.Security;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Pki;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "InfisicalScepDynamicChallenge")]
    [OutputType(typeof(SecureString))]
    [OutputType(typeof(string))]
    public sealed class NewInfisicalScepDynamicChallengeCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [Alias("Id")]
        public string ApplicationId { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        [Alias("CertificateProfileId")]
        public string ProfileId { get; set; }

        [Parameter] public SwitchParameter AsPlainText { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);

                string challenge = client.GenerateScepDynamicChallenge(connection, ApplicationId, ProfileId);
                if (AsPlainText.IsPresent)
                {
                    WriteObject(challenge);
                    return;
                }

                SecureString secure = new SecureString();
                foreach (char c in challenge) { secure.AppendChar(c); }
                secure.MakeReadOnly();
                WriteObject(secure);
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("NewInfisicalScepDynamicChallengeCmdlet", "GenerateScepDynamicChallenge", exception);
            }
        }
    }
}
