using System;
using System.Text.RegularExpressions;

namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalProcessResult
    {
        public string FilePath { get; set; }
        public int? ProcessId { get; set; }
        public int? ExitCode { get; set; }
        public string ExitCodeAsHex { get; set; }
        public int? ExitCodeAsInteger { get; set; }
        public string ExitCodeAsDecimal { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public string DurationFriendly { get; set; }
        public string StandardOutput { get; set; }
        public string StandardError { get; set; }
        public Match[] StandardOutputObject { get; set; }
        public Match[] StandardErrorObject { get; set; }
        public bool TimedOut { get; set; }
        public bool Succeeded { get; set; }
        public int SecretCount { get; set; }
    }
}
