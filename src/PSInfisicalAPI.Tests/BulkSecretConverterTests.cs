using System.Collections;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Secrets;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class BulkSecretConverterTests
    {
        [Fact]
        public void ToCreateItems_Maps_Standard_Keys()
        {
            Hashtable entry = new Hashtable
            {
                { "SecretName", "API_KEY" },
                { "SecretValue", "abc" },
                { "SecretComment", "primary" },
                { "SkipMultilineEncoding", true },
                { "TagIds", new[] { "tag-1", "tag-2" } }
            };

            InfisicalBulkCreateSecretItem[] items = InfisicalBulkSecretConverter.ToCreateItems(new[] { entry });
            Assert.Single(items);
            Assert.Equal("API_KEY", items[0].SecretName);
            Assert.Equal("abc", items[0].SecretValue);
            Assert.Equal("primary", items[0].SecretComment);
            Assert.True(items[0].SkipMultilineEncoding);
            Assert.Equal(new[] { "tag-1", "tag-2" }, items[0].TagIds);
        }

        [Fact]
        public void ToCreateItems_Accepts_Name_Alias_For_SecretName()
        {
            Hashtable entry = new Hashtable
            {
                { "Name", "API_KEY" },
                { "Value", "abc" }
            };

            InfisicalBulkCreateSecretItem[] items = InfisicalBulkSecretConverter.ToCreateItems(new[] { entry });
            Assert.Single(items);
            Assert.Equal("API_KEY", items[0].SecretName);
            Assert.Equal("abc", items[0].SecretValue);
        }

        [Fact]
        public void ToCreateItems_Without_SecretName_Throws()
        {
            Hashtable entry = new Hashtable { { "Value", "abc" } };

            Assert.Throws<InfisicalConfigurationException>(() =>
                InfisicalBulkSecretConverter.ToCreateItems(new[] { entry }));
        }

        [Fact]
        public void ToUpdateItems_Maps_NewSecretName()
        {
            Hashtable entry = new Hashtable
            {
                { "SecretName", "API_KEY" },
                { "NewSecretName", "RENAMED" },
                { "SecretValue", "new-value" }
            };

            InfisicalBulkUpdateSecretItem[] items = InfisicalBulkSecretConverter.ToUpdateItems(new[] { entry });
            Assert.Single(items);
            Assert.Equal("API_KEY", items[0].SecretName);
            Assert.Equal("RENAMED", items[0].NewSecretName);
            Assert.Equal("new-value", items[0].SecretValue);
        }

        [Fact]
        public void ToCreateItems_Maps_Metadata_Dictionary()
        {
            Hashtable meta = new Hashtable { { "owner", "platform" }, { "tier", "1" } };
            Hashtable entry = new Hashtable
            {
                { "SecretName", "API_KEY" },
                { "SecretValue", "abc" },
                { "Metadata", meta }
            };

            InfisicalBulkCreateSecretItem[] items = InfisicalBulkSecretConverter.ToCreateItems(new[] { entry });
            Assert.NotNull(items[0].SecretMetadata);
            Assert.Equal("platform", items[0].SecretMetadata["owner"]);
            Assert.Equal("1", items[0].SecretMetadata["tier"]);
        }

        [Fact]
        public void ToCreateItems_Empty_Input_Returns_Empty()
        {
            InfisicalBulkCreateSecretItem[] items = InfisicalBulkSecretConverter.ToCreateItems(null);
            Assert.Empty(items);
        }
    }
}
