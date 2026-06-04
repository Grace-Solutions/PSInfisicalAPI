using System;
using System.Collections.Generic;
using PSInfisicalAPI.Errors;

namespace PSInfisicalAPI.Secrets
{
    public static class InfisicalBulkSecretConverter
    {
        public static InfisicalBulkCreateSecretItem[] ToCreateItems(IEnumerable<IDictionary<string, string>> input)
        {
            if (input == null) { return new InfisicalBulkCreateSecretItem[0]; }

            List<InfisicalBulkCreateSecretItem> list = new List<InfisicalBulkCreateSecretItem>();
            foreach (IDictionary<string, string> entry in input)
            {
                if (entry == null) { continue; }
                IDictionary<string, string> table = Normalize(entry);
                InfisicalBulkCreateSecretItem item = new InfisicalBulkCreateSecretItem
                {
                    SecretName = GetString(table, "SecretName", "Name", "Key", "SecretKey"),
                    SecretValue = GetString(table, "SecretValue", "Value"),
                    SecretComment = GetString(table, "SecretComment", "Comment"),
                    SkipMultilineEncoding = GetBool(table, "SkipMultilineEncoding"),
                    TagIds = GetStringArray(table, "TagIds")
                };

                if (string.IsNullOrEmpty(item.SecretName))
                {
                    throw new InfisicalConfigurationException("Each bulk-create entry must include 'SecretName' (or 'Name'/'Key').");
                }

                list.Add(item);
            }

            return list.ToArray();
        }

        public static InfisicalBulkUpdateSecretItem[] ToUpdateItems(IEnumerable<IDictionary<string, string>> input)
        {
            if (input == null) { return new InfisicalBulkUpdateSecretItem[0]; }

            List<InfisicalBulkUpdateSecretItem> list = new List<InfisicalBulkUpdateSecretItem>();
            foreach (IDictionary<string, string> entry in input)
            {
                if (entry == null) { continue; }
                IDictionary<string, string> table = Normalize(entry);
                InfisicalBulkUpdateSecretItem item = new InfisicalBulkUpdateSecretItem
                {
                    SecretName = GetString(table, "SecretName", "Name", "Key", "SecretKey"),
                    NewSecretName = GetString(table, "NewSecretName", "NewName"),
                    SecretValue = GetString(table, "SecretValue", "Value"),
                    SecretComment = GetString(table, "SecretComment", "Comment"),
                    SkipMultilineEncoding = GetBool(table, "SkipMultilineEncoding"),
                    TagIds = GetStringArray(table, "TagIds")
                };

                if (string.IsNullOrEmpty(item.SecretName))
                {
                    throw new InfisicalConfigurationException("Each bulk-update entry must include 'SecretName' (or 'Name'/'Key').");
                }

                list.Add(item);
            }

            return list.ToArray();
        }

        private static IDictionary<string, string> Normalize(IDictionary<string, string> source)
        {
            Dictionary<string, string> normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> kvp in source)
            {
                if (string.IsNullOrEmpty(kvp.Key)) { continue; }
                normalized[kvp.Key] = kvp.Value;
            }

            return normalized;
        }

        private static string GetString(IDictionary<string, string> table, params string[] keys)
        {
            foreach (string key in keys)
            {
                string value;
                if (table.TryGetValue(key, out value) && !string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return null;
        }

        private static bool? GetBool(IDictionary<string, string> table, string key)
        {
            string value;
            if (!table.TryGetValue(key, out value) || string.IsNullOrEmpty(value)) { return null; }
            bool parsed;
            return bool.TryParse(value, out parsed) ? parsed : (bool?)null;
        }

        private static string[] GetStringArray(IDictionary<string, string> table, string key)
        {
            string value;
            if (!table.TryGetValue(key, out value) || string.IsNullOrEmpty(value)) { return null; }
            string[] parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> trimmed = new List<string>(parts.Length);
            foreach (string part in parts)
            {
                string item = part.Trim();
                if (!string.IsNullOrEmpty(item)) { trimmed.Add(item); }
            }

            return trimmed.Count == 0 ? null : trimmed.ToArray();
        }
    }
}
