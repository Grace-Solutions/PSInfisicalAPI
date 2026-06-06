using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Process;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Start, "InfisicalProcess", DefaultParameterSetName = WindowStyleSet, SupportsShouldProcess = true)]
    [OutputType(typeof(InfisicalProcessResult))]
    public sealed class StartInfisicalProcessCmdlet : InfisicalCmdletBase
    {
        private const string Component = "StartInfisicalProcessCmdlet";
        private const string WindowStyleSet = "WindowStyle";
        private const string CreateNoWindowSet = "CreateNoWindow";

        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        [Alias("FP")]
        public string FilePath { get; set; }

        [Parameter]
        [Alias("WD")]
        public DirectoryInfo WorkingDirectory { get; set; }

        [Parameter]
        [AllowEmptyCollection]
        [AllowNull]
        [Alias("AL")]
        public string[] ArgumentList { get; set; }

        [Parameter]
        [AllowEmptyCollection]
        [AllowNull]
        [Alias("AECL")]
        public string[] AcceptableExitCodeList { get; set; }

        [Parameter(ParameterSetName = WindowStyleSet)]
        [ValidateSet("Normal", "Hidden", "Minimized", "Maximized")]
        [Alias("WS")]
        public string WindowStyle { get; set; } = "Hidden";

        [Parameter(ParameterSetName = CreateNoWindowSet)]
        [Alias("CNW")]
        public SwitchParameter CreateNoWindow { get; set; }

        [Parameter]
        [Alias("NW")]
        public SwitchParameter NoWait { get; set; }

        [Parameter]
        [ValidateSet("AboveNormal", "BelowNormal", "High", "Idle", "Normal", "RealTime")]
        [Alias("P")]
        public string Priority { get; set; } = "Normal";

        [Parameter]
        [Alias("ET")]
        public TimeSpan ExecutionTimeout { get; set; }

        [Parameter]
        [Alias("ETI")]
        public TimeSpan ExecutionTimeoutInterval { get; set; } = TimeSpan.FromSeconds(15);

        [Parameter]
        [Alias("SIO")]
        public object[] StandardInputObjectList { get; set; }

        [Parameter]
        [Alias("ENV")]
        public IDictionary EnvironmentVariables { get; set; }

        [Parameter]
        [Alias("StandardOutputParsingExpression", "SOPE", "PE")]
        public Regex ParsingExpression { get; set; }

        [Parameter]
        [Alias("SAL")]
        public SwitchParameter SecureArgumentList { get; set; }

        [Parameter]
        [Alias("LO")]
        public SwitchParameter LogOutput { get; set; }

        [Parameter]
        [Alias("COE")]
        public SwitchParameter ContinueOnError { get; set; }

        [Parameter(ValueFromPipeline = true)]
        [Alias("Secrets", "InputObject")]
        public InfisicalSecret[] Secret { get; set; }

        [Parameter]
        public string Prefix { get; set; }

        private readonly List<InfisicalSecret> _secretBuffer = new List<InfisicalSecret>();

        protected override void ProcessRecord()
        {
            if (Secret == null) { return; }
            foreach (InfisicalSecret secret in Secret)
            {
                if (secret != null) { _secretBuffer.Add(secret); }
            }
        }

        protected override void EndProcessing()
        {
            try
            {
                string target = string.IsNullOrEmpty(WorkingDirectory != null ? WorkingDirectory.FullName : null)
                    ? FilePath
                    : string.Concat(FilePath, " (in ", WorkingDirectory.FullName, ")");

                if (!ShouldProcess(target, "Start process with Infisical secrets")) { return; }

                InfisicalProcessOptions options = new InfisicalProcessOptions
                {
                    FilePath = FilePath,
                    WorkingDirectory = WorkingDirectory,
                    ArgumentList = ArgumentList,
                    AcceptableExitCodeList = AcceptableExitCodeList,
                    WindowStyle = WindowStyle,
                    CreateNoWindow = CreateNoWindow.IsPresent,
                    NoWait = NoWait.IsPresent,
                    Priority = Priority,
                    ExecutionTimeout = MyInvocation.BoundParameters.ContainsKey("ExecutionTimeout") ? (TimeSpan?)ExecutionTimeout : null,
                    ExecutionTimeoutInterval = ExecutionTimeoutInterval,
                    StandardInputObjectList = StandardInputObjectList,
                    EnvironmentVariables = EnvironmentVariables,
                    ParsingExpression = ParsingExpression,
                    SecureArgumentList = SecureArgumentList.IsPresent,
                    LogOutput = LogOutput.IsPresent,
                    ContinueOnError = ContinueOnError.IsPresent,
                    Secrets = _secretBuffer.ToArray(),
                    Prefix = Prefix
                };

                InfisicalProcessResult result = InfisicalProcessRunner.Run(options, Logger);
                WriteObject(result);

                if (!result.Succeeded && !NoWait.IsPresent && !ContinueOnError.IsPresent)
                {
                    string message = string.Concat("Process '", FilePath, "' exited with code ", result.ExitCode.HasValue ? result.ExitCode.Value.ToString() : "<null>", " which is not in the acceptable exit code list.");
                    InvalidOperationException exception = new InvalidOperationException(message);
                    ErrorRecord error = new ErrorRecord(exception, "StartInfisicalProcess.UnacceptableExitCode", ErrorCategory.InvalidResult, result);
                    ThrowTerminatingError(error);
                }
            }
            catch (PipelineStoppedException) { throw; }
            catch (Exception exception)
            {
                ThrowTerminatingForException(Component, "StartProcess", exception);
            }
        }
    }
}
