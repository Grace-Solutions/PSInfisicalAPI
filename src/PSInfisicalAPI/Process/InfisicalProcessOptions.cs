using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Process
{
    public sealed class InfisicalProcessOptions
    {
        public string FilePath { get; set; }
        public DirectoryInfo WorkingDirectory { get; set; }
        public string[] ArgumentList { get; set; }
        public string[] AcceptableExitCodeList { get; set; }
        public string WindowStyle { get; set; }
        public bool CreateNoWindow { get; set; }
        public bool NoWait { get; set; }
        public string Priority { get; set; }
        public TimeSpan? ExecutionTimeout { get; set; }
        public TimeSpan ExecutionTimeoutInterval { get; set; }
        public object[] StandardInputObjectList { get; set; }
        public IDictionary EnvironmentVariables { get; set; }
        public Regex ParsingExpression { get; set; }
        public bool SecureArgumentList { get; set; }
        public bool LogOutput { get; set; }
        public bool ContinueOnError { get; set; }
        public InfisicalSecret[] Secrets { get; set; }
        public string Prefix { get; set; }
    }
}
