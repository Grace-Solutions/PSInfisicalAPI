using System.Collections.Generic;
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
            Dictionary<string, string> entry = new Dictionary<string, string>
            {
                { "SecretName", "API_KEY" },
                { "SecretValue", "abc" },
                { "SecretComment", "primary" },
                { "SkipMultilineEncoding", "true" },
                { "TagIds", "tag-1,tag-2" }
            };

            InfisicalBulkCreateSecretItem[] items = InfisicalBulkSecretConverter.ToCreateItems(new IDictionary<string, string>[] { entry });
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
            Dictionary<string, string> entry = new Dictionary<string, string>
            {
                { "Name", "API_KEY" },
                { "Value", "abc" }
            };

            InfisicalBulkCreateSecretItem[] items = InfisicalBulkSecretConverter.ToCreateItems(new IDictionary<string, string>[] { entry });
            Assert.Single(items);
            Assert.Equal("API_KEY", items[0].SecretName);
            Assert.Equal("abc", items[0].SecretValue);
        }

        [Fact]
        public void ToCreateItems_Without_SecretName_Throws()
        {
            Dictionary<string, string> entry = new Dictionary<string, string> { { "Value", "abc" } };

            Assert.Throws<InfisicalConfigurationException>(() =>
                InfisicalBulkSecretConverter.ToCreateItems(new IDictionary<string, string>[] { entry }));
        }

        [Fact]
        public void ToUpdateItems_Maps_NewSecretName()
        {
            Dictionary<string, string> entry = new Dictionary<string, string>
            {
                { "SecretName", "API_KEY" },
                { "NewSecretName", "RENAMED" },
                { "SecretValue", "new-value" }
            };

            InfisicalBulkUpdateSecretItem[] items = InfisicalBulkSecretConverter.ToUpdateItems(new IDictionary<string, string>[] { entry });
            Assert.Single(items);
            Assert.Equal("API_KEY", items[0].SecretName);
            Assert.Equal("RENAMED", items[0].NewSecretName);
            Assert.Equal("new-value", items[0].SecretValue);
        }

        [Fact]
        public void ToCreateItems_Trims_TagId_Whitespace_And_Skips_Empty()
        {
            Dictionary<string, string> entry = new Dictionary<string, string>
            {
                { "SecretName", "API_KEY" },
                { "SecretValue", "abc" },
                { "TagIds", " tag-1 , , tag-2 " }
            };

            InfisicalBulkCreateSecretItem[] items = InfisicalBulkSecretConverter.ToCreateItems(new IDictionary<string, string>[] { entry });
            Assert.Equal(new[] { "tag-1", "tag-2" }, items[0].TagIds);
        }

        [Fact]
        public void ToCreateItems_Empty_Input_Returns_Empty()
        {
            InfisicalBulkCreateSecretItem[] items = InfisicalBulkSecretConverter.ToCreateItems(null);
            Assert.Empty(items);
        }
    }
}
