using System;

namespace PSInfisicalAPI.Common
{
    public static class InfisicalPrefix
    {
        public static string Apply(string original, string prefix, bool force)
        {
            if (string.IsNullOrEmpty(prefix)) { return original ?? string.Empty; }
            if (original == null) { return prefix; }
            if (!force && original.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) { return original; }
            return string.Concat(prefix, original);
        }
    }
}
