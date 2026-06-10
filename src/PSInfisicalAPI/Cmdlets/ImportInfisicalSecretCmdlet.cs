using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Security;
using PSInfisicalAPI.Common;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Imports;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Security;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsData.Import, "InfisicalSecret")]
    [OutputType(typeof(Dictionary<string, SecureString>))]
    [OutputType(typeof(Dictionary<string, string>))]
    public sealed class ImportInfisicalSecretCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNull]
        public FileInfo Path { get; set; }

        [Parameter(Mandatory = true)]
        public InfisicalImportFormat Format { get; set; }

        [Parameter]
        public InfisicalDuplicateKeyBehavior DuplicateKeyBehavior { get; set; } = InfisicalDuplicateKeyBehavior.Error;

        [Parameter]
        public SwitchParameter AsPlainText { get; set; }

        [Parameter]
        [Alias("Prefix")]
        public string SecretsPrefix { get; set; }

        [Parameter]
        [Alias("ForcePrefix")]
        public SwitchParameter ForceSecretsPrefix { get; set; }

        protected override void EndProcessing()
        {
            try
            {
                Path.Refresh();
                if (!Path.Exists)
                {
                    throw new InfisicalImportException(string.Concat("Import path does not exist: ", Path.FullName));
                }

                IInfisicalImporter importer = InfisicalImporterFactory.Create(Format);
                IList<KeyValuePair<string, string>> pairs = importer.Import(Path);

                if (AsPlainText.IsPresent)
                {
                    Dictionary<string, string> plain = BuildDictionary<string>(pairs, value => value ?? string.Empty);
                    WriteObject(plain);
                }
                else
                {
                    Dictionary<string, SecureString> secure = BuildDictionary<SecureString>(pairs, value => SecureStringUtility.ToReadOnlySecureString(value ?? string.Empty));
                    WriteObject(secure);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("ImportInfisicalSecretCmdlet", "ImportSecret", exception);
            }
        }

        private Dictionary<string, TValue> BuildDictionary<TValue>(
            IList<KeyValuePair<string, string>> pairs,
            Func<string, TValue> valueSelector)
        {
            Dictionary<string, TValue> dictionary = new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, string> pair in pairs)
            {
                if (pair.Key == null) { continue; }
                string key = InfisicalPrefix.Apply(pair.Key, SecretsPrefix, ForceSecretsPrefix.IsPresent);

                if (dictionary.ContainsKey(key))
                {
                    if (DuplicateKeyBehavior == InfisicalDuplicateKeyBehavior.Error)
                    {
                        throw new InfisicalConfigurationException(string.Concat("Duplicate secret name encountered: ", key));
                    }

                    if (DuplicateKeyBehavior == InfisicalDuplicateKeyBehavior.LastWins)
                    {
                        dictionary[key] = valueSelector(pair.Value);
                    }

                    continue;
                }

                dictionary[key] = valueSelector(pair.Value);
            }

            return dictionary;
        }
    }
}
