using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsData.Export, "InfisicalScepMdmProfile", SupportsShouldProcess = true)]
    [OutputType(typeof(FileInfo))]
    public sealed class ExportInfisicalScepMdmProfileCmdlet : InfisicalCmdletBase
    {
        private const string Component = "ExportInfisicalScepMdmProfileCmdlet";

        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        [Alias("Profile", "ScepProfile")]
        public InfisicalScepMdmProfile InputObject { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public string Path { get; set; }

        [Parameter] public SwitchParameter Force { get; set; }
        [Parameter] public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (InputObject == null) { throw new InvalidOperationException("InputObject is required."); }

                string resolvedPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(Path);
                if (File.Exists(resolvedPath) && !Force.IsPresent)
                {
                    Logger.Warning(Component, string.Concat("File '", resolvedPath, "' already exists. Pass -Force to overwrite. Skipping export."));
                    return;
                }

                if (!ShouldProcess(resolvedPath, "Write SyncML SCEP MDM profile"))
                {
                    return;
                }

                string directory = System.IO.Path.GetDirectoryName(resolvedPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Logger.Verbose(Component, string.Concat("Created directory '", directory, "'."));
                }

                string syncMl = InputObject.ToSyncMl();
                File.WriteAllText(resolvedPath, syncMl, new UTF8Encoding(false));

                Logger.Information(Component, string.Concat("Wrote SCEP MDM profile to '", resolvedPath, "' (UniqueId=", InputObject.UniqueId, ")."));
                if (PassThru.IsPresent)
                {
                    WriteObject(new FileInfo(resolvedPath));
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException(Component, "ExportScepMdmProfile", exception);
            }
        }
    }
}
