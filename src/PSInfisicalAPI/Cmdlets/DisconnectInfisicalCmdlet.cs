using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommunications.Disconnect, "Infisical")]
    [OutputType(typeof(PSObject))]
    public sealed class DisconnectInfisicalCmdlet : InfisicalCmdletBase
    {
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalSessionManager.Disconnect();

                if (PassThru.IsPresent)
                {
                    PSObject status = new PSObject();
                    status.Properties.Add(new PSNoteProperty("IsConnected", false));
                    status.Properties.Add(new PSNoteProperty("DisconnectedAtUtc", DateTimeOffset.UtcNow));
                    WriteObject(status);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("DisconnectInfisicalCmdlet", "Disconnect", exception);
            }
        }
    }
}
