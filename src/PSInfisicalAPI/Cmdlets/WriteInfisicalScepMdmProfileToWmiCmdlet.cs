using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Text;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "InfisicalScepMdmProfileToWmi", SupportsShouldProcess = true)]
    [OutputType(typeof(PSObject))]
    public sealed class WriteInfisicalScepMdmProfileToWmiCmdlet : InfisicalCmdletBase
    {
        private const string Component = "WriteInfisicalScepMdmProfileToWmiCmdlet";

        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        [Alias("Profile", "ScepProfile")]
        public InfisicalScepMdmProfile InputObject { get; set; }

        [Parameter] public string Namespace { get; set; } = "root/cimv2/mdm/dmmap";
        [Parameter] public string ClassName { get; set; } = "MDM_ClientCertificateInstall_SCEP02";
        [Parameter] public SwitchParameter SkipElevationCheck { get; set; }
        [Parameter] public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (InputObject == null) { throw new InvalidOperationException("InputObject is required."); }
                if (string.IsNullOrEmpty(InputObject.UniqueId)) { throw new InvalidOperationException("InputObject.UniqueId is required."); }
                if (string.IsNullOrEmpty(InputObject.ServerUrl)) { throw new InvalidOperationException("InputObject.ServerUrl is required."); }

                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                {
                    throw new PlatformNotSupportedException("Write-InfisicalScepMdmProfileToWmi requires Windows (MDM Bridge WMI provider).");
                }

                bool deviceScope = !string.Equals(InputObject.Scope, "User", StringComparison.OrdinalIgnoreCase);
                if (deviceScope && !SkipElevationCheck.IsPresent && !IsElevated())
                {
                    throw new UnauthorizedAccessException("Device-scope SCEP enrollment requires an elevated session (run as Administrator or SYSTEM). Pass -SkipElevationCheck to bypass this guard.");
                }

                string parentId = string.Concat("./Vendor/MSFT/ClientCertificateInstall/SCEP/", InputObject.UniqueId);
                Hashtable properties = BuildProperties(InputObject, parentId);
                string target = string.Concat(Namespace, " ", ClassName, " ParentID=", parentId);
                if (!ShouldProcess(target, "New-CimInstance MDM SCEP enrollment"))
                {
                    return;
                }

                Logger.Verbose(Component, string.Concat("Creating CIM instance in namespace '", Namespace, "' for class '", ClassName, "' with ParentID '", parentId, "'."));
                Collection<PSObject> results = InvokeNewCimInstance(Namespace, ClassName, properties);

                Logger.Information(Component, string.Concat("Submitted SCEP MDM profile '", InputObject.UniqueId, "' to MDM Bridge WMI provider (results=", results != null ? results.Count : 0, ")."));
                if (PassThru.IsPresent && results != null)
                {
                    foreach (PSObject result in results) { WriteObject(result); }
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException(Component, "WriteScepMdmProfileToWmi", exception);
            }
        }

        private bool IsElevated()
        {
            try
            {
                Collection<PSObject> results = InvokeCommand.InvokeScript("[bool]([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)");
                if (results == null || results.Count == 0 || results[0] == null || results[0].BaseObject == null) { return false; }
                return Convert.ToBoolean(results[0].BaseObject, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Logger.Verbose(Component, string.Concat("Elevation check failed; assuming non-elevated. ", ex.Message));
                return false;
            }
        }

        private Collection<PSObject> InvokeNewCimInstance(string ns, string className, Hashtable properties)
        {
            Dictionary<string, object> variables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "ns", ns },
                { "class", className },
                { "props", properties }
            };

            foreach (KeyValuePair<string, object> kv in variables)
            {
                SessionState.PSVariable.Set(kv.Key, kv.Value);
            }

            try
            {
                return InvokeCommand.InvokeScript("New-CimInstance -Namespace $ns -ClassName $class -Property $props -ErrorAction Stop");
            }
            finally
            {
                foreach (KeyValuePair<string, object> kv in variables)
                {
                    SessionState.PSVariable.Remove(kv.Key);
                }
            }
        }

        private static Hashtable BuildProperties(InfisicalScepMdmProfile profile, string parentId)
        {
            Hashtable h = new Hashtable(StringComparer.OrdinalIgnoreCase);
            h["ParentID"] = parentId;
            h["InstanceID"] = "Install";

            AddString(h, "ServerURL", profile.ServerUrl);
            AddString(h, "Challenge", profile.Challenge);
            AddString(h, "SubjectName", profile.SubjectName);
            AddString(h, "SubjectAlternativeNames", profile.SubjectAlternativeNames);
            AddString(h, "EKUMapping", profile.EkuMapping);
            AddInt(h, "KeyUsage", profile.KeyUsage);
            AddInt(h, "KeyLength", profile.KeyLength);
            AddString(h, "KeyAlgorithm", profile.KeyAlgorithm);
            AddString(h, "HashAlgorithm", profile.HashAlgorithm);
            AddInt(h, "KeyProtection", profile.KeyProtection);
            AddString(h, "ContainerName", profile.ContainerName);
            AddString(h, "ValidPeriod", profile.ValidPeriod);
            AddInt(h, "ValidPeriodUnits", profile.ValidPeriodUnits);
            AddInt(h, "RetryCount", profile.RetryCount);
            AddInt(h, "RetryDelay", profile.RetryDelay);
            AddString(h, "TemplateName", profile.TemplateName);
            AddString(h, "CAThumbprint", profile.CAThumbprint);
            AddString(h, "CustomTextToShowInPrompt", profile.CustomTextToShowInPrompt);

            return h;
        }

        private static void AddString(Hashtable h, string key, string value)
        {
            if (string.IsNullOrEmpty(value)) { return; }
            h[key] = value;
        }

        private static void AddInt(Hashtable h, string key, int? value)
        {
            if (!value.HasValue) { return; }
            h[key] = value.Value;
        }
    }
}
