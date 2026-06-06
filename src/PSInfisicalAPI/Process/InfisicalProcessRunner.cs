using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Models;
using SystemProcess = System.Diagnostics.Process;
using static PSInfisicalAPI.Process.InfisicalProcessRunnerHelpers;

namespace PSInfisicalAPI.Process
{
    public static class InfisicalProcessRunner
    {
        private const string Component = "InfisicalProcessRunner";

        public static InfisicalProcessResult Run(InfisicalProcessOptions options, IInfisicalLogger logger)
        {
            if (options == null) { throw new ArgumentNullException(nameof(options)); }
            if (string.IsNullOrWhiteSpace(options.FilePath)) { throw new ArgumentException("FilePath is required.", nameof(options)); }

            string[] acceptable = (options.AcceptableExitCodeList != null && options.AcceptableExitCodeList.Length > 0)
                ? options.AcceptableExitCodeList
                : new[] { "0", "3010" };

            InfisicalProcessResult result = new InfisicalProcessResult
            {
                FilePath = options.FilePath,
                ExitCode = -1,
                SecretCount = options.Secrets != null ? options.Secrets.Length : 0
            };

            SystemProcess process = new SystemProcess();
            process.StartInfo.FileName = options.FilePath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;

            if (options.CreateNoWindow) { process.StartInfo.CreateNoWindow = true; }
            else if (!string.IsNullOrWhiteSpace(options.WindowStyle))
            {
                process.StartInfo.WindowStyle = (ProcessWindowStyle)Enum.Parse(typeof(ProcessWindowStyle), options.WindowStyle, true);
            }

            if (options.WorkingDirectory != null && !string.IsNullOrWhiteSpace(options.WorkingDirectory.FullName))
            {
                if (!Directory.Exists(options.WorkingDirectory.FullName))
                {
                    Directory.CreateDirectory(options.WorkingDirectory.FullName);
                }
                process.StartInfo.WorkingDirectory = options.WorkingDirectory.FullName;
            }

            ApplyEnvironment(process.StartInfo.Environment, options, logger);

            if (options.ArgumentList != null && options.ArgumentList.Length > 0)
            {
                process.StartInfo.Arguments = string.Join(" ", options.ArgumentList);
            }

            LogCommand(process.StartInfo, options, logger);
            LogVerbose(logger, string.Concat("Acceptable exit codes: ", string.Join("; ", acceptable)));

            Stopwatch timer = Stopwatch.StartNew();
            process.Start();
            result.ProcessId = process.Id;
            try { result.StartTime = process.StartTime; } catch { }

            ApplyPriority(process, options.Priority, logger);

            StringBuilder stdoutBuffer = new StringBuilder();
            StringBuilder stderrBuffer = new StringBuilder();
            object stdoutGate = new object();
            object stderrGate = new object();
            process.OutputDataReceived += (sender, args) => { if (args.Data != null) { lock (stdoutGate) { stdoutBuffer.AppendLine(args.Data); } } };
            process.ErrorDataReceived += (sender, args) => { if (args.Data != null) { lock (stderrGate) { stderrBuffer.AppendLine(args.Data); } } };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            WriteStandardInput(process, options.StandardInputObjectList, options.SecureArgumentList, logger);

            if (options.NoWait)
            {
                LogVerbose(logger, string.Concat("Skipping wait for process ID ", process.Id, "."));
                return result;
            }

            WaitForExit(process, options.ExecutionTimeout, options.ExecutionTimeoutInterval, timer, result, logger);

            try { process.WaitForExit(); } catch { }
            lock (stdoutGate) { result.StandardOutput = stdoutBuffer.Length > 0 ? stdoutBuffer.ToString() : null; }
            lock (stderrGate) { result.StandardError = stderrBuffer.Length > 0 ? stderrBuffer.ToString() : null; }

            try { result.ExitCode = process.ExitCode; } catch { result.ExitCode = null; }
            FormatExitCodes(result);
            try { result.ExitTime = process.ExitTime; } catch { }
            if (result.StartTime.HasValue && result.ExitTime.HasValue) { result.Duration = result.ExitTime.Value - result.StartTime.Value; }
            if (result.Duration.HasValue)
            {
                result.DurationFriendly = FormatFriendly(result.Duration.Value);
                LogVerbose(logger, string.Concat("The command execution took ", result.DurationFriendly, "."));
            }

            ApplyRegex(result, options.ParsingExpression);
            result.Succeeded = IsAcceptable(result, acceptable);

            try { process.Dispose(); } catch { }

            if (options.LogOutput || !result.Succeeded)
            {
                LogVerbose(logger, string.Concat("StandardOutput: ", string.IsNullOrEmpty(result.StandardOutput) ? "N/A" : result.StandardOutput));
                LogVerbose(logger, string.Concat("StandardError: ", string.IsNullOrEmpty(result.StandardError) ? "N/A" : result.StandardError));
            }

            return result;
        }

        private static bool IsAcceptable(InfisicalProcessResult result, string[] acceptable)
        {
            if (acceptable.Any(c => string.Equals(c, "*", StringComparison.Ordinal))) { return true; }
            HashSet<string> set = new HashSet<string>(acceptable, StringComparer.OrdinalIgnoreCase);
            if (result.ExitCode.HasValue && set.Contains(result.ExitCode.Value.ToString(CultureInfo.InvariantCulture))) { return true; }
            if (!string.IsNullOrEmpty(result.ExitCodeAsHex) && set.Contains(result.ExitCodeAsHex)) { return true; }
            if (result.ExitCodeAsInteger.HasValue && set.Contains(result.ExitCodeAsInteger.Value.ToString(CultureInfo.InvariantCulture))) { return true; }
            if (!string.IsNullOrEmpty(result.ExitCodeAsDecimal) && set.Contains(result.ExitCodeAsDecimal)) { return true; }
            return false;
        }

        private static void FormatExitCodes(InfisicalProcessResult result)
        {
            if (!result.ExitCode.HasValue) { return; }
            try
            {
                result.ExitCodeAsHex = "0x" + Convert.ToString(result.ExitCode.Value, 16).PadLeft(8, '0').ToUpperInvariant();
                int parsed;
                if (int.TryParse(result.ExitCodeAsHex.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parsed))
                {
                    result.ExitCodeAsInteger = parsed;
                }
                result.ExitCodeAsDecimal = result.ExitCode.Value.ToString(CultureInfo.InvariantCulture);
            }
            catch { }
        }

        private static void LogVerbose(IInfisicalLogger logger, string message)
        {
            if (logger != null) { logger.Verbose(Component, message); }
        }
    }
}
