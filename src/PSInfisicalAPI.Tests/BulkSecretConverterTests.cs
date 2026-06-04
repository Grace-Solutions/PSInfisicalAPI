using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Management.Automation;
using PSInfisicalAPI.Cmdlets;
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

    public class BulkSecretsTransformationAttributeTests
    {
        private static IDictionary<string, string>[] Transform(object input)
        {
            BulkSecretsTransformationAttribute attribute = new BulkSecretsTransformationAttribute();
            object result = attribute.Transform(null, input);
            return (IDictionary<string, string>[])result;
        }

        [Fact]
        public void Transform_Single_Hashtable_Wraps_Into_Array()
        {
            Hashtable input = new Hashtable { { "SecretName", "API_KEY" }, { "SecretValue", "abc" } };

            IDictionary<string, string>[] result = Transform(input);

            Assert.Single(result);
            Assert.Equal("API_KEY", result[0]["SecretName"]);
            Assert.Equal("abc", result[0]["SecretValue"]);
        }

        [Fact]
        public void Transform_Hashtable_Array_Preserves_Order()
        {
            Hashtable a = new Hashtable { { "SecretName", "A" }, { "SecretValue", "1" } };
            Hashtable b = new Hashtable { { "SecretName", "B" }, { "SecretValue", "2" } };

            IDictionary<string, string>[] result = Transform(new object[] { a, b });

            Assert.Equal(2, result.Length);
            Assert.Equal("A", result[0]["SecretName"]);
            Assert.Equal("B", result[1]["SecretName"]);
        }

        [Fact]
        public void Transform_OrderedDictionary_Converts_To_StringString()
        {
            OrderedDictionary input = new OrderedDictionary();
            input.Add("SecretName", "API_KEY");
            input.Add("SkipMultilineEncoding", true);

            IDictionary<string, string>[] result = Transform(input);

            Assert.Single(result);
            Assert.Equal("API_KEY", result[0]["SecretName"]);
            Assert.Equal("true", result[0]["SkipMultilineEncoding"]);
        }

        [Fact]
        public void Transform_Array_Values_Are_Joined_Comma_Separated()
        {
            Hashtable input = new Hashtable
            {
                { "SecretName", "API_KEY" },
                { "TagIds", new[] { "tag-1", "tag-2" } }
            };

            IDictionary<string, string>[] result = Transform(input);

            Assert.Equal("tag-1,tag-2", result[0]["TagIds"]);
        }

        [Fact]
        public void Transform_Already_Typed_Array_Passes_Through()
        {
            IDictionary<string, string>[] input = new IDictionary<string, string>[]
            {
                new Dictionary<string, string> { { "SecretName", "A" }, { "SecretValue", "1" } }
            };

            IDictionary<string, string>[] result = Transform(input);

            Assert.Same(input, result);
        }

        [Fact]
        public void Transform_Null_Input_Returns_Null()
        {
            BulkSecretsTransformationAttribute attribute = new BulkSecretsTransformationAttribute();
            Assert.Null(attribute.Transform(null, null));
        }

        [Fact]
        public void Transform_Invalid_Element_Throws_ArgumentTransformationMetadataException()
        {
            BulkSecretsTransformationAttribute attribute = new BulkSecretsTransformationAttribute();
            Assert.Throws<ArgumentTransformationMetadataException>(() =>
                attribute.Transform(null, new object[] { "not-a-dictionary" }));
        }

        [Fact]
        public void Transform_End_To_End_With_Converter_Produces_Bulk_Items()
        {
            Hashtable entry = new Hashtable
            {
                { "SecretName", "API_KEY" },
                { "SecretValue", "abc" },
                { "TagIds", new[] { "tag-1", "tag-2" } },
                { "SkipMultilineEncoding", true }
            };

            IDictionary<string, string>[] transformed = Transform(new object[] { entry });
            InfisicalBulkCreateSecretItem[] items = InfisicalBulkSecretConverter.ToCreateItems(transformed);

            Assert.Single(items);
            Assert.Equal("API_KEY", items[0].SecretName);
            Assert.Equal("abc", items[0].SecretValue);
            Assert.Equal(new[] { "tag-1", "tag-2" }, items[0].TagIds);
            Assert.True(items[0].SkipMultilineEncoding);
        }
    }
}
