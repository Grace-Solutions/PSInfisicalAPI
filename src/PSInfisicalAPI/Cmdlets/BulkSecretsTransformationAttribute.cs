using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using PSInfisicalAPI.Security;

namespace PSInfisicalAPI.Cmdlets
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class BulkSecretsTransformationAttribute : ArgumentTransformationAttribute
    {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            if (inputData == null) { return null; }

            object unwrapped = Unwrap(inputData);

            if (unwrapped is IDictionary<string, string>[] strongArray) { return strongArray; }

            if (unwrapped is IDictionary singleDict)
            {
                return new IDictionary<string, string>[] { Convert(singleDict) };
            }

            if (unwrapped is IEnumerable enumerable && !(unwrapped is string))
            {
                List<IDictionary<string, string>> result = new List<IDictionary<string, string>>();
                foreach (object element in enumerable)
                {
                    if (element == null) { continue; }
                    object e = Unwrap(element);
                    if (e is IDictionary dict)
                    {
                        result.Add(Convert(dict));
                        continue;
                    }

                    throw new ArgumentTransformationMetadataException(
                        "Each element of -Secrets must be a dictionary (Hashtable, OrderedDictionary, Dictionary<string,string>, etc.).");
                }

                return result.ToArray();
            }

            throw new ArgumentTransformationMetadataException(
                "-Secrets must be a dictionary or an array of dictionaries.");
        }

        private static object Unwrap(object value)
        {
            PSObject pso = value as PSObject;
            return pso != null ? pso.BaseObject : value;
        }

        private static IDictionary<string, string> Convert(IDictionary source)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in source)
            {
                if (entry.Key == null) { continue; }
                string key = entry.Key.ToString();
                dict[key] = Stringify(entry.Value);
            }

            return dict;
        }

        private static string Stringify(object value)
        {
            object v = Unwrap(value);
            if (v == null) { return null; }
            if (v is string s) { return s; }
            if (v is bool b) { return b ? "true" : "false"; }
            if (v is System.Security.SecureString secure)
            {
                return SecureStringUtility.UsePlainText(secure, plain => plain);
            }

            if (v is IEnumerable enumerable)
            {
                List<string> parts = new List<string>();
                foreach (object item in enumerable)
                {
                    if (item == null) { continue; }
                    parts.Add(Unwrap(item).ToString());
                }

                return string.Join(",", parts);
            }

            return v.ToString();
        }
    }
}
