using System.Collections.Generic;
using System.IO;
using PSInfisicalAPI.Errors;

namespace PSInfisicalAPI.Imports
{
    public sealed class EnvInfisicalImporter : IInfisicalImporter
    {
        public IList<KeyValuePair<string, string>> Import(FileInfo path)
        {
            if (path == null) { throw new InfisicalImportException("Path is required for ENV import."); }

            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            string[] lines = File.ReadAllLines(path.FullName);

            foreach (string raw in lines)
            {
                if (raw == null) { continue; }
                string line = raw.Trim();
                if (line.Length == 0) { continue; }
                if (line[0] == '#') { continue; }

                int idx = line.IndexOf('=');
                if (idx <= 0) { continue; }

                string key = line.Substring(0, idx).Trim();
                string value = line.Substring(idx + 1);
                if (key.Length == 0) { continue; }

                result.Add(new KeyValuePair<string, string>(key, value));
            }

            return result;
        }
    }
}
