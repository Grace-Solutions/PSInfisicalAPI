using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Text.RegularExpressions;
using PSInfisicalAPI.Logging;

namespace PSInfisicalAPI.Authentication
{
    public static class InfisicalEnvironmentResolver
    {
        public const string Component = "EnvironmentResolver";

        private const RegexOptions DefaultRegexOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

        private static readonly EnvironmentVariableTarget[] ScopeOrder = new[]
        {
            EnvironmentVariableTarget.Process,
            EnvironmentVariableTarget.User,
            EnvironmentVariableTarget.Machine
        };

        public static readonly Regex[] BaseUriPatterns = new[]
        {
            new Regex(@".*INFISICAL.*API.*URL.*", DefaultRegexOptions),
            new Regex(@".*INFISICAL.*BASE.*UR(I|L).*", DefaultRegexOptions),
            new Regex(@".*INFISICAL.*HOST.*", DefaultRegexOptions),
            new Regex(@".*INFISICAL.*URL.*", DefaultRegexOptions)
        };

        public static readonly Regex[] OrganizationIdPatterns = new[]
        {
            new Regex(@".*INFISICAL.*ORG(ANIZATION)?.*ID.*", DefaultRegexOptions)
        };

        public static readonly Regex[] ClientIdPatterns = new[]
        {
            new Regex(@".*INFISICAL.*CLIENT.*ID.*", DefaultRegexOptions),
            new Regex(@".*INFISICAL.*(UNIVERSAL.*AUTH|MACHINE.*IDENTITY).*ID.*", DefaultRegexOptions)
        };

        public static readonly Regex[] ClientSecretPatterns = new[]
        {
            new Regex(@".*INFISICAL.*CLIENT.*SECRET.*", DefaultRegexOptions),
            new Regex(@".*INFISICAL.*(UNIVERSAL.*AUTH|MACHINE.*IDENTITY).*SECRET.*", DefaultRegexOptions)
        };

        public static readonly Regex[] AccessTokenPatterns = new[]
        {
            new Regex(@".*INFISICAL.*ACCESS.*TOKEN.*", DefaultRegexOptions),
            new Regex(@".*INFISICAL.*AUTH.*TOKEN.*", DefaultRegexOptions),
            new Regex(@".*INFISICAL.*TOKEN.*", DefaultRegexOptions)
        };

        public static readonly Regex[] ApiVersionPatterns = new[]
        {
            new Regex(@".*INFISICAL.*API.*VERSION.*", DefaultRegexOptions)
        };

        public sealed class ResolutionResult
        {
            public bool Found { get; set; }
            public string Value { get; set; }
            public string VariableName { get; set; }
            public EnvironmentVariableTarget Scope { get; set; }
        }

        public static ResolutionResult Resolve(IEnumerable<Regex> patterns)
        {
            if (patterns == null)
            {
                return new ResolutionResult { Found = false };
            }

            List<Regex> patternList = new List<Regex>(patterns);

            for (int i = 0; i < ScopeOrder.Length; i++)
            {
                EnvironmentVariableTarget scope = ScopeOrder[i];
                IDictionary entries;

                try
                {
                    entries = Environment.GetEnvironmentVariables(scope);
                }
                catch (SecurityException)
                {
                    continue;
                }
                catch (PlatformNotSupportedException)
                {
                    continue;
                }

                if (entries == null)
                {
                    continue;
                }

                foreach (DictionaryEntry entry in entries)
                {
                    string name = entry.Key as string;
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    string value = entry.Value as string;
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    for (int p = 0; p < patternList.Count; p++)
                    {
                        if (patternList[p].IsMatch(name))
                        {
                            return new ResolutionResult
                            {
                                Found = true,
                                Value = value,
                                VariableName = name,
                                Scope = scope
                            };
                        }
                    }
                }
            }

            return new ResolutionResult { Found = false };
        }

        public static string ResolveString(string parameterName, IEnumerable<Regex> patterns, string currentValue, IInfisicalLogger logger)
        {
            if (!string.IsNullOrWhiteSpace(currentValue))
            {
                return currentValue;
            }

            ResolutionResult result = Resolve(patterns);
            if (!result.Found)
            {
                return currentValue;
            }

            if (logger != null)
            {
                logger.Verbose(Component, string.Format("Resolved {0} from environment variable '{1}' ({2} scope).", parameterName, result.VariableName, result.Scope));
            }

            return result.Value;
        }

        public static SecureString ResolveSecureString(string parameterName, IEnumerable<Regex> patterns, SecureString currentValue, IInfisicalLogger logger)
        {
            if (currentValue != null && currentValue.Length > 0)
            {
                return currentValue;
            }

            ResolutionResult result = Resolve(patterns);
            if (!result.Found)
            {
                return currentValue;
            }

            if (logger != null)
            {
                logger.Verbose(Component, string.Format("Resolved {0} from environment variable '{1}' ({2} scope).", parameterName, result.VariableName, result.Scope));
            }

            SecureString secureString = new SecureString();
            string plainText = result.Value;
            try
            {
                for (int i = 0; i < plainText.Length; i++)
                {
                    secureString.AppendChar(plainText[i]);
                }
            }
            finally
            {
                plainText = null;
                result.Value = null;
            }

            secureString.MakeReadOnly();
            return secureString;
        }
    }
}
