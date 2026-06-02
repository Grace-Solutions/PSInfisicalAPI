using System;
using System.IO;
using System.Text;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Exports
{
    public sealed class InfisicalExportRequest
    {
        public InfisicalSecret[] Secrets { get; set; }
        public InfisicalExportFormat Format { get; set; }
        public FileInfo Path { get; set; }
        public EnvironmentVariableTarget Scope { get; set; }
        public bool Force { get; set; }
        public Encoding Encoding { get; set; }
    }
}
