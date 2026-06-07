using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using PSInfisicalAPI.Errors;

namespace PSInfisicalAPI.Imports
{
    public sealed class JsonInfisicalImporter : IInfisicalImporter
    {
        public IList<KeyValuePair<string, string>> Import(FileInfo path)
        {
            if (path == null) { throw new InfisicalImportException("Path is required for JSON import."); }

            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            string text = File.ReadAllText(path.FullName);
            JToken root = JToken.Parse(text);

            if (root.Type == JTokenType.Array)
            {
                foreach (JToken item in (JArray)root)
                {
                    if (item == null || item.Type != JTokenType.Object) { continue; }
                    string key = ReadString((JObject)item, "SecretName") ?? ReadString((JObject)item, "secretName");
                    string value = ReadString((JObject)item, "SecretValue") ?? ReadString((JObject)item, "secretValue");
                    if (string.IsNullOrEmpty(key)) { continue; }
                    result.Add(new KeyValuePair<string, string>(key, value ?? string.Empty));
                }
            }
            else if (root.Type == JTokenType.Object)
            {
                foreach (JProperty prop in ((JObject)root).Properties())
                {
                    if (prop == null || string.IsNullOrEmpty(prop.Name)) { continue; }
                    string value = prop.Value != null && prop.Value.Type != JTokenType.Null ? prop.Value.ToString() : string.Empty;
                    result.Add(new KeyValuePair<string, string>(prop.Name, value));
                }
            }
            else
            {
                throw new InfisicalImportException("JSON import expects an array of secret objects or a flat key/value object.");
            }

            return result;
        }

        private static string ReadString(JObject obj, string name)
        {
            JToken token;
            if (!obj.TryGetValue(name, out token) || token == null || token.Type == JTokenType.Null) { return null; }
            return token.ToString();
        }
    }
}
