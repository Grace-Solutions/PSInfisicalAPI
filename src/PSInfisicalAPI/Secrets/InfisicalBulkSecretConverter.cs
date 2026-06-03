using System;
using System.Collections;
using System.Collections.Generic;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Security;

namespace PSInfisicalAPI.Secrets
{
    public static class InfisicalBulkSecretConverter
    {
        public static InfisicalBulkCreateSecretItem[] ToCreateItems(IEnumerable input)
        {
            if (input == null) { return new InfisicalBulkCreateSecretItem[0]; }

            List<InfisicalBulkCreateSecretItem> list = new List<InfisicalBulkCreateSecretItem>();
            foreach (object element in input)
            {
                Hashtable table = AsHashtable(element);
                InfisicalBulkCreateSecretItem item = new InfisicalBulkCreateSecretItem
                {
                    SecretName = GetString(table, "SecretName", "Name", "Key", "SecretKey"),
                    SecretValue = GetSecretValue(table, "SecretValue", "Value"),
                    SecretComment = GetString(table, "SecretComment", "Comment"),
                    SkipMultilineEncoding = GetBool(table, "SkipMultilineEncoding"),
                    TagIds = GetStringArray(table, "TagIds"),
                    SecretMetadata = GetStringDictionary(table, "SecretMetadata", "Metadata")
                };

                if (string.IsNullOrEmpty(item.SecretName))
                {
                    throw new InfisicalConfigurationException("Each bulk-create entry must include 'SecretName' (or 'Name'/'Key').");
                }

                list.Add(item);
            }

            return list.ToArray();
        }

        public static InfisicalBulkUpdateSecretItem[] ToUpdateItems(IEnumerable input)
        {
            if (input == null) { return new InfisicalBulkUpdateSecretItem[0]; }

            List<InfisicalBulkUpdateSecretItem> list = new List<InfisicalBulkUpdateSecretItem>();
            foreach (object element in input)
            {
                Hashtable table = AsHashtable(element);
                InfisicalBulkUpdateSecretItem item = new InfisicalBulkUpdateSecretItem
                {
                    SecretName = GetString(table, "SecretName", "Name", "Key", "SecretKey"),
                    NewSecretName = GetString(table, "NewSecretName", "NewName"),
                    SecretValue = GetSecretValue(table, "SecretValue", "Value"),
                    SecretComment = GetString(table, "SecretComment", "Comment"),
                    SkipMultilineEncoding = GetBool(table, "SkipMultilineEncoding"),
                    TagIds = GetStringArray(table, "TagIds"),
                    SecretMetadata = GetStringDictionary(table, "SecretMetadata", "Metadata")
                };

                if (string.IsNullOrEmpty(item.SecretName))
                {
                    throw new InfisicalConfigurationException("Each bulk-update entry must include 'SecretName' (or 'Name'/'Key').");
                }

                list.Add(item);
            }

            return list.ToArray();
        }

        private static Hashtable AsHashtable(object element)
        {
            if (element is Hashtable hashtable) { return hashtable; }
            if (element is IDictionary dictionary)
            {
                Hashtable converted = new Hashtable(StringComparer.OrdinalIgnoreCase);
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (entry.Key == null) { continue; }
                    converted[entry.Key.ToString()] = entry.Value;
                }

                return converted;
            }

            throw new InfisicalConfigurationException("Bulk secret entries must be Hashtable or IDictionary values.");
        }

        private static string GetString(Hashtable table, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (table.ContainsKey(key) && table[key] != null)
                {
                    return table[key].ToString();
                }
            }

            return null;
        }

        private static string GetSecretValue(Hashtable table, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (!table.ContainsKey(key)) { continue; }
                object value = table[key];
                if (value == null) { return null; }
                if (value is System.Security.SecureString secure)
                {
                    return SecureStringUtility.UsePlainText(secure, plain => plain);
                }

                return value.ToString();
            }

            return null;
        }

        private static bool? GetBool(Hashtable table, string key)
        {
            if (!table.ContainsKey(key) || table[key] == null) { return null; }
            object value = table[key];
            if (value is bool b) { return b; }
            bool parsed;
            return bool.TryParse(value.ToString(), out parsed) ? parsed : (bool?)null;
        }

        private static string[] GetStringArray(Hashtable table, string key)
        {
            if (!table.ContainsKey(key) || table[key] == null) { return null; }
            object value = table[key];
            if (value is string[] direct) { return direct; }
            if (value is IEnumerable enumerable && !(value is string))
            {
                List<string> items = new List<string>();
                foreach (object item in enumerable) { if (item != null) { items.Add(item.ToString()); } }
                return items.ToArray();
            }

            return new[] { value.ToString() };
        }

        private static Dictionary<string, string> GetStringDictionary(Hashtable table, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (!table.ContainsKey(key) || table[key] == null) { continue; }
                if (table[key] is IDictionary dictionary)
                {
                    Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        if (entry.Key == null) { continue; }
                        result[entry.Key.ToString()] = entry.Value != null ? entry.Value.ToString() : null;
                    }

                    return result;
                }
            }

            return null;
        }
    }
}
