using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Pki;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalPkiSubscriber", DefaultParameterSetName = "List")]
    [OutputType(typeof(InfisicalPkiSubscriber))]
    public sealed class GetInfisicalPkiSubscriberCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "ByName", Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [Alias("SubscriberName", "Slug")]
        public string Name { get; set; }

        [Parameter(Mandatory = true)] public string ProjectId { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);

                if (string.Equals(ParameterSetName, "ByName", StringComparison.Ordinal))
                {
                    InfisicalPkiSubscriber subscriber = client.GetPkiSubscriber(connection, Name, ProjectId);
                    if (subscriber != null)
                    {
                        WriteObject(subscriber);
                    }

                    return;
                }

                InfisicalPkiSubscriber[] all = client.ListPkiSubscribers(connection, ProjectId);
                foreach (InfisicalPkiSubscriber subscriber in all)
                {
                    WriteObject(subscriber);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalPkiSubscriberCmdlet", "GetPkiSubscriber", exception);
            }
        }
    }
}
