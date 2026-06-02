using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Exports
{
    public static class InfisicalExporterFactory
    {
        public static IInfisicalExporter Create(InfisicalExportFormat format)
        {
            switch (format)
            {
                case InfisicalExportFormat.Json: return new JsonInfisicalExporter();
                case InfisicalExportFormat.Yaml: return new YamlInfisicalExporter();
                case InfisicalExportFormat.Env: return new EnvInfisicalExporter();
                case InfisicalExportFormat.Xml: return new XmlInfisicalExporter();
                case InfisicalExportFormat.EnvironmentVariables: return new EnvironmentVariableExporter();
                default: throw new InfisicalExportException(string.Concat("Unsupported export format: ", format.ToString()));
            }
        }
    }
}
