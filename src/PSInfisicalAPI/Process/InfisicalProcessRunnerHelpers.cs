using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Security;
using SystemProcess = System.Diagnostics.Process;

namespace PSInfisicalAPI.Process
{
    internal static class InfisicalProcessRunnerHelpers
    {
        private const string Component = "InfisicalProcessRunner";

        internal static void ApplyEnvironment(IDictionary<string, string> processEnv, InfisicalProcessOptions options, IInfisicalLogger logger)
        {
            if (processEnv == null) { return; }

            if (options.EnvironmentVariables != null && options.EnvironmentVariables.Count > 0)
            {
                Log(logger, string.Concat("Injecting ", options.EnvironmentVariables.Count, " explicit environment variable(s) into the process."));
                foreach (DictionaryEntry entry in options.EnvironmentVariables)
                {
                    if (entry.Key == null) { continue; }
                    string key = entry.Key.ToString();
                    string value = entry.Value != null ? entry.Value.ToString() : string.Empty;
                    processEnv[key] = value;
                }
            }

            if (options.Secrets == null || options.Secrets.Length == 0) { return; }

            Log(logger, string.Concat("Injecting ", options.Secrets.Length, " Infisical secret(s) into the process environment."));
            foreach (InfisicalSecret secret in options.Secrets)
            {
                if (secret == null || string.IsNullOrEmpty(secret.SecretName) || secret.SecretValue == null) { continue; }
                string name = string.IsNullOrEmpty(options.Prefix) ? secret.SecretName : string.Concat(options.Prefix, secret.SecretName);
                SecureStringUtility.UsePlainText(secret.SecretValue, plain =>
                {
                    processEnv[name] = plain;
                    return true;
                });
            }
        }

        internal static void LogCommand(ProcessStartInfo startInfo, InfisicalProcessOptions options, IInfisicalLogger logger)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Attempting to execute: ");
            builder.Append(startInfo.FileName);

            if (!string.IsNullOrEmpty(startInfo.Arguments))
            {
                builder.Append(' ');
                if (options.SecureArgumentList)
                {
                    int len = Math.Min(20, Math.Max(5, startInfo.Arguments.Length));
                    builder.Append(new string('*', len));
                }
                else
                {
                    builder.Append(startInfo.Arguments);
                }
            }

            Log(logger, builder.ToString());
        }

        internal static void ApplyPriority(SystemProcess process, string priority, IInfisicalLogger logger)
        {
            if (string.IsNullOrWhiteSpace(priority)) { return; }
            ProcessPriorityClass desired;
            if (!Enum.TryParse(priority, true, out desired)) { return; }
            try
            {
                if (process.PriorityClass != desired)
                {
                    process.PriorityClass = desired;
                    Log(logger, string.Concat("Set process priority class to '", desired, "' for process ID ", process.Id, "."));
                }
            }
            catch (Exception exception)
            {
                Log(logger, string.Concat("Unable to set process priority class to '", desired, "': ", exception.Message));
            }
        }

        internal static void WriteStandardInput(SystemProcess process, object[] inputs, bool secureArguments, IInfisicalLogger logger)
        {
            if (inputs == null || inputs.Length == 0) { return; }
            for (int i = 0; i < inputs.Length; i++)
            {
                try
                {
                    object value = inputs[i];
                    string preview = secureArguments ? new string('*', 8) : (value != null ? value.ToString() : string.Empty);
                    Log(logger, string.Concat("Writing standard input object ", i + 1, " of ", inputs.Length, " to process ID ", process.Id, ": ", preview));
                    process.StandardInput.WriteLine(value);
                }
                catch (Exception exception)
                {
                    Log(logger, string.Concat("Failed to write standard input object ", i + 1, ": ", exception.Message));
                }
            }
        }

        internal static void WaitForExit(SystemProcess process, TimeSpan? timeout, TimeSpan interval, Stopwatch timer, InfisicalProcessResult result, IInfisicalLogger logger)
        {
            TimeSpan pollInterval = interval.TotalMilliseconds > 0 ? interval : TimeSpan.FromSeconds(15);
            int processId = process.Id;

            if (!timeout.HasValue)
            {
                Log(logger, string.Concat("A timeout was not specified for process ID ", processId, "."));
                Log(logger, string.Concat("The wait for process ID ", processId, " termination will be indefinite."));
            }
            else
            {
                Log(logger, string.Concat("Process timeout duration: ", FormatFriendly(timeout.Value)));
            }

            while (!GetProcessHasExited(processId))
            {
                Log(logger, string.Concat("Process ID ", processId, " has been running for ", FormatFriendly(timer.Elapsed), "."));

                if (timeout.HasValue && timer.Elapsed >= timeout.Value)
                {
                    Log(logger, string.Concat("Process ID ", processId, " exceeded the maximum timeout duration of ", FormatFriendly(timeout.Value), "; terminating."));
                    try { process.Kill(); result.TimedOut = true; } catch { }
                    break;
                }

                Log(logger, string.Concat("Checking again in another ", FormatFriendly(pollInterval), ". Please wait..."));
                System.Threading.Thread.Sleep(pollInterval);
            }
        }

        private static bool GetProcessHasExited(int processId)
        {
            try { return SystemProcess.GetProcessById(processId).HasExited; }
            catch { return true; }
        }

        internal static string FormatFriendly(TimeSpan value)
        {
            string[] names = new[] { "Days", "Hours", "Minutes", "Seconds", "Milliseconds" };
            int[] values = new[] { value.Days, value.Hours, value.Minutes, value.Seconds, value.Milliseconds };

            List<int> nonZeroIndices = new List<int>();
            for (int i = 0; i < values.Length; i++) { if (values[i] > 0) { nonZeroIndices.Add(i); } }

            if (nonZeroIndices.Count == 0) { return "N/A"; }

            StringBuilder builder = new StringBuilder();
            int last = nonZeroIndices.Count - 1;
            for (int i = 0; i < nonZeroIndices.Count; i++)
            {
                int index = nonZeroIndices[i];
                int amount = values[index];
                string name = names[index].ToLowerInvariant();
                if (amount == 1) { name = name.TrimEnd('s'); }

                if (nonZeroIndices.Count > 1 && i == last) { builder.Append("and "); }
                builder.Append(amount);
                builder.Append(' ');
                builder.Append(name);
                if (nonZeroIndices.Count > 1 && i != last) { builder.Append(", "); }
            }

            return builder.ToString();
        }

        internal static void ApplyRegex(InfisicalProcessResult result, Regex expression)
        {
            if (expression == null) { return; }
            if (!string.IsNullOrEmpty(result.StandardOutput))
            {
                result.StandardOutputObject = expression.Matches(result.StandardOutput).Cast<Match>().ToArray();
            }
            if (!string.IsNullOrEmpty(result.StandardError))
            {
                result.StandardErrorObject = expression.Matches(result.StandardError).Cast<Match>().ToArray();
            }
        }

        private static void Log(IInfisicalLogger logger, string message)
        {
            if (logger != null) { logger.Verbose(Component, message); }
        }
    }
}
