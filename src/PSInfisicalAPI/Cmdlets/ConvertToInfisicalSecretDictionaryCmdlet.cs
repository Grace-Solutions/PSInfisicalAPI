using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security;
using PSInfisicalAPI.Common;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsData.ConvertTo, "InfisicalSecretDictionary")]
    [OutputType(typeof(Dictionary<string, SecureString>))]
    [OutputType(typeof(Dictionary<string, string>))]
    public sealed class ConvertToInfisicalSecretDictionaryCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public InfisicalSecret[] InputObject { get; set; }

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

        private readonly List<InfisicalSecret> _buffer = new List<InfisicalSecret>();

        protected override void ProcessRecord()
        {
            if (InputObject == null) { return; }

            foreach (InfisicalSecret secret in InputObject)
            {
                if (secret != null)
                {
                    _buffer.Add(secret);
                }
            }
        }

        protected override void EndProcessing()
        {
            try
            {
                if (AsPlainText.IsPresent)
                {
                    Dictionary<string, string> plain = BuildDictionary<string>(secret => secret.GetPlainTextValue());
                    WriteObject(plain);
                }
                else
                {
                    Dictionary<string, SecureString> secure = BuildDictionary<SecureString>(secret => secret.SecretValue);
                    WriteObject(secure);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("ConvertToInfisicalSecretDictionaryCmdlet", "ConvertToDictionary", exception);
            }
        }

        private Dictionary<string, TValue> BuildDictionary<TValue>(Func<InfisicalSecret, TValue> valueSelector)
        {
            Dictionary<string, TValue> dictionary = new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);

            foreach (InfisicalSecret secret in _buffer)
            {
                string key = InfisicalPrefix.Apply(secret.SecretName ?? string.Empty, SecretsPrefix, ForceSecretsPrefix.IsPresent);

                if (dictionary.ContainsKey(key))
                {
                    if (DuplicateKeyBehavior == InfisicalDuplicateKeyBehavior.Error)
                    {
                        throw new InfisicalConfigurationException(string.Concat("Duplicate secret name encountered: ", key));
                    }

                    if (DuplicateKeyBehavior == InfisicalDuplicateKeyBehavior.LastWins)
                    {
                        dictionary[key] = valueSelector(secret);
                    }

                    continue;
                }

                dictionary[key] = valueSelector(secret);
            }

            return dictionary;
        }
    }
}
