using System.Reflection;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Security;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class InfisicalSecretTests
    {
        [Fact]
        public void ToString_Returns_SecretName_Only()
        {
            InfisicalSecret secret = new InfisicalSecret
            {
                SecretName = "MySecret",
                SecretValue = SecureStringUtility.ToReadOnlySecureString("super-secret")
            };

            Assert.Equal("MySecret", secret.ToString());
            Assert.DoesNotContain("super-secret", secret.ToString());
        }

        [Fact]
        public void UsePlainTextValue_Scopes_Plaintext_Access()
        {
            InfisicalSecret secret = new InfisicalSecret
            {
                SecretName = "DbPassword",
                SecretValue = SecureStringUtility.ToReadOnlySecureString("p@ssw0rd")
            };

            string observed = secret.UsePlainTextValue(value => value);
            Assert.Equal("p@ssw0rd", observed);
        }

        [Fact]
        public void Has_No_PlainText_Property()
        {
            PropertyInfo[] properties = typeof(InfisicalSecret).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                Assert.False(string.Equals(property.Name, "PlainTextValue", System.StringComparison.OrdinalIgnoreCase));
                if (property.Name == "SecretValue")
                {
                    Assert.Equal(typeof(System.Security.SecureString), property.PropertyType);
                }
            }
        }
    }
}
