using System.Reflection;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Security;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class SecretMapperTests
    {
        [Fact]
        public void Mapper_Maps_SecretKey_To_SecretName_And_SecretValue_To_SecureString()
        {
            System.Type mapperType = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly
                .GetType("PSInfisicalAPI.Secrets.InfisicalSecretMapper", true);
            System.Type dtoType = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly
                .GetType("PSInfisicalAPI.Secrets.InfisicalSecretResponseDto", true);

            object dto = System.Activator.CreateInstance(dtoType);
            dtoType.GetProperty("SecretKey").SetValue(dto, "DatabasePassword");
            dtoType.GetProperty("SecretValue").SetValue(dto, "Sup3rSecret!");
            dtoType.GetProperty("SecretPath").SetValue(dto, "/prod/db");
            dtoType.GetProperty("Environment").SetValue(dto, "prod");
            dtoType.GetProperty("Type").SetValue(dto, "shared");

            MethodInfo mapMethod = mapperType.GetMethod("Map", BindingFlags.Public | BindingFlags.Static);
            InfisicalSecret secret = (InfisicalSecret)mapMethod.Invoke(null, new[] { dto });

            Assert.Equal("DatabasePassword", secret.SecretName);
            Assert.NotNull(secret.SecretValue);
            Assert.True(secret.SecretValue.IsReadOnly());
            Assert.Equal("/prod/db", secret.SecretPath);
            Assert.Equal(InfisicalSecretType.Shared, secret.Type);

            string roundtripped = SecureStringUtility.UsePlainText(secret.SecretValue, plain => plain);
            Assert.Equal("Sup3rSecret!", roundtripped);
        }
    }
}
