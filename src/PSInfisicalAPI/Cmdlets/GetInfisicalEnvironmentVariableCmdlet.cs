using System;
using System.Management.Automation;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalEnvironmentVariable")]
    [OutputType(typeof(string))]
    public sealed class GetInfisicalEnvironmentVariableCmdlet : PSCmdlet
    {
        private static readonly EnvironmentVariableTarget[] TargetOrder = new[]
        {
            EnvironmentVariableTarget.Process,
            EnvironmentVariableTarget.User,
            EnvironmentVariableTarget.Machine
        };

        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            foreach (EnvironmentVariableTarget target in TargetOrder)
            {
                string value;
                try
                {
                    value = Environment.GetEnvironmentVariable(Name, target);
                }
                catch
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    WriteObject(value);
                    return;
                }
            }
        }
    }
}
