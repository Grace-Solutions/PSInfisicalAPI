using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.SubOrganizations;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalSubOrganization", DefaultParameterSetName = "List")]
    [OutputType(typeof(InfisicalSubOrganization))]
    public sealed class GetInfisicalSubOrganizationCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "Single", Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Id")]
        public string SubOrganizationId { get; set; }

        [Parameter(ParameterSetName = "List")] public int? Limit { get; set; }
        [Parameter(ParameterSetName = "List")] public int? Offset { get; set; }
        [Parameter(ParameterSetName = "List")] public string Search { get; set; }
        [Parameter(ParameterSetName = "List")] public string OrderBy { get; set; }

        [Parameter(ParameterSetName = "List")]
        [ValidateSet("asc", "desc")]
        public string OrderDirection { get; set; }

        [Parameter(ParameterSetName = "List")] public SwitchParameter IsAccessible { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalSubOrganizationClient client = new InfisicalSubOrganizationClient(HttpClient, Logger);

                if (string.Equals(ParameterSetName, "Single", StringComparison.Ordinal))
                {
                    InfisicalSubOrganization subOrganization = client.Retrieve(connection, SubOrganizationId);
                    if (subOrganization != null)
                    {
                        WriteObject(subOrganization);
                    }

                    return;
                }

                bool? isAccessible = MyInvocation.BoundParameters.ContainsKey("IsAccessible") ? (bool?)IsAccessible.IsPresent : null;
                InfisicalSubOrganization[] subOrganizations = client.List(connection, Limit, Offset, Search, OrderBy, OrderDirection, isAccessible);
                foreach (InfisicalSubOrganization subOrganization in subOrganizations)
                {
                    WriteObject(subOrganization);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalSubOrganizationCmdlet", "GetSubOrganization", exception);
            }
        }
    }
}
