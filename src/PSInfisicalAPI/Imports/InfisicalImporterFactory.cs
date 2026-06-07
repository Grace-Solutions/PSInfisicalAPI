using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Imports
{
    public static class InfisicalImporterFactory
    {
        public static IInfisicalImporter Create(InfisicalImportFormat format)
        {
            switch (format)
            {
                case InfisicalImportFormat.Json: return new JsonInfisicalImporter();
                case InfisicalImportFormat.Yaml: return new YamlInfisicalImporter();
                case InfisicalImportFormat.Env: return new EnvInfisicalImporter();
                case InfisicalImportFormat.Xml: return new XmlInfisicalImporter();
                default: throw new InfisicalImportException(string.Concat("Unsupported import format: ", format.ToString()));
            }
        }
    }
}
