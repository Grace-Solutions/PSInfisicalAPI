using System;
using System.Security;
using PSInfisicalAPI.Security;

namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalSecret
    {
        public string Id { get; set; }
        public string InternalId { get; set; }
        public string Workspace { get; set; }
        public string Environment { get; set; }
        public int? Version { get; set; }
        public InfisicalSecretType Type { get; set; }
        public string SecretName { get; set; }
        public SecureString SecretValue { get; set; }
        public bool SecretValueHidden { get; set; }
        public string SecretPath { get; set; }
        public string SecretComment { get; set; }
        public DateTimeOffset? CreatedAtUtc { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }
        public bool IsRotatedSecret { get; set; }
        public Guid? RotationId { get; set; }
        public InfisicalSecretTag[] Tags { get; set; }
        public InfisicalSecretMetadata[] SecretMetadata { get; set; }

        public T UsePlainTextValue<T>(Func<string, T> action)
        {
            return SecureStringUtility.UsePlainText(SecretValue, action);
        }

        public void UsePlainTextValue(Action<string> action)
        {
            SecureStringUtility.UsePlainText(SecretValue, action);
        }

        public string GetPlainTextValue()
        {
            if (SecretValue == null) { return null; }
            return SecureStringUtility.UsePlainText(SecretValue, plainText => plainText);
        }

        public override string ToString()
        {
            return SecretName;
        }
    }
}
