using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalSANList")]
    [OutputType(typeof(string[]))]
    public sealed class GetInfisicalSANListCmdlet : InfisicalCmdletBase
    {
        private const string Component = "GetInfisicalSANListCmdlet";

        [Parameter]
        [ValidateNotNullOrEmpty]
        public string InclusionExpression { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        public string ExclusionExpression { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                Regex includeRegex = !string.IsNullOrEmpty(InclusionExpression) ? new Regex(InclusionExpression, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) : null;
                Regex excludeRegex = !string.IsNullOrEmpty(ExclusionExpression) ? new Regex(ExclusionExpression, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) : null;

                List<string> sans = new List<string>();
                HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                string deviceName = Dns.GetHostName();
                AddUnique(sans, seen, deviceName);

                HashSet<string> suffixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                string globalDomain = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                if (!string.IsNullOrEmpty(globalDomain)) { suffixes.Add(globalDomain); }

                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in adapters)
                {
                    if (adapter.OperationalStatus != OperationalStatus.Up) { continue; }
                    if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback) { continue; }

                    IPInterfaceProperties props = adapter.GetIPProperties();
                    if (!string.IsNullOrEmpty(props.DnsSuffix)) { suffixes.Add(props.DnsSuffix); }

                    foreach (UnicastIPAddressInformation unicast in props.UnicastAddresses)
                    {
                        IPAddress ip = unicast.Address;
                        if (ip.AddressFamily == AddressFamily.InterNetwork && IsRfc1918OrCgnat(ip))
                        {
                            AddUnique(sans, seen, ip.ToString());
                        }
                    }
                }

                foreach (string suffix in suffixes)
                {
                    string trimmed = suffix.Trim().TrimStart('.');
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        AddUnique(sans, seen, string.Concat(deviceName, ".", trimmed));
                    }
                }

                AddUnique(sans, seen, "127.0.0.1");
                AddUnique(sans, seen, "::1");

                List<string> filtered = new List<string>(sans.Count);
                foreach (string san in sans)
                {
                    if (includeRegex != null && !includeRegex.IsMatch(san)) { continue; }
                    if (excludeRegex != null && excludeRegex.IsMatch(san)) { continue; }
                    filtered.Add(san);
                }

                WriteObject(filtered.ToArray(), false);
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException(Component, "GetSANList", exception);
            }
        }

        private static bool IsRfc1918OrCgnat(IPAddress ip)
        {
            byte[] bytes = ip.GetAddressBytes();
            if (bytes.Length != 4) { return false; }

            if (bytes[0] == 10) { return true; }
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) { return true; }
            if (bytes[0] == 192 && bytes[1] == 168) { return true; }
            if (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127) { return true; }

            return false;
        }

        private static void AddUnique(List<string> list, HashSet<string> seen, string value)
        {
            if (string.IsNullOrEmpty(value)) { return; }
            if (seen.Add(value)) { list.Add(value); }
        }
    }
}
