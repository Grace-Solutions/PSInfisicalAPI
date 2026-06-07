using System.Collections;
using System.Collections.Generic;
using System.IO;
using PSInfisicalAPI.Errors;
using YamlDotNet.Serialization;

namespace PSInfisicalAPI.Imports
{
    public sealed class YamlInfisicalImporter : IInfisicalImporter
    {
        public IList<KeyValuePair<string, string>> Import(FileInfo path)
        {
            if (path == null) { throw new InfisicalImportException("Path is required for YAML import."); }

            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            string text = File.ReadAllText(path.FullName);

            IDeserializer deserializer = new DeserializerBuilder().Build();
            object root = deserializer.Deserialize<object>(text);
            if (root == null) { return result; }

            IDictionary rootMap = root as IDictionary;
            if (rootMap != null && rootMap.Contains("Secrets"))
            {
                IList entries = rootMap["Secrets"] as IList;
                if (entries != null)
                {
                    foreach (object entry in entries)
                    {
                        IDictionary map = entry as IDictionary;
                        if (map == null) { continue; }
                        string key = AsString(map["SecretName"]);
                        string value = AsString(map["SecretValue"]);
                        if (string.IsNullOrEmpty(key)) { continue; }
                        result.Add(new KeyValuePair<string, string>(key, value ?? string.Empty));
                    }
                    return result;
                }
            }

            if (rootMap != null)
            {
                foreach (DictionaryEntry kvp in rootMap)
                {
                    string key = AsString(kvp.Key);
                    if (string.IsNullOrEmpty(key)) { continue; }
                    result.Add(new KeyValuePair<string, string>(key, AsString(kvp.Value) ?? string.Empty));
                }
                return result;
            }

            throw new InfisicalImportException("YAML import expects a 'Secrets' root list or a flat key/value mapping.");
        }

        private static string AsString(object value)
        {
            if (value == null) { return null; }
            return value.ToString();
        }
    }
}
