using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class BulkSecretDtoTests
    {
        private static readonly System.Reflection.Assembly ModuleAssembly =
            typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly;

        private static object MakeDto(string typeName)
        {
            System.Type t = ModuleAssembly.GetType(typeName, true);
            return System.Activator.CreateInstance(t);
        }

        [Fact]
        public void BatchCreateItem_Serializes_With_Expected_Field_Names()
        {
            object item = MakeDto("PSInfisicalAPI.Secrets.InfisicalSecretBatchCreateItemDto");
            item.GetType().GetProperty("SecretKey").SetValue(item, "API_KEY");
            item.GetType().GetProperty("SecretValue").SetValue(item, "abc");

            JObject json = JObject.Parse(JsonConvert.SerializeObject(item));
            Assert.Equal("API_KEY", (string)json["secretKey"]);
            Assert.Equal("abc", (string)json["secretValue"]);
            Assert.False(json.ContainsKey("skipMultilineEncoding"));
            Assert.False(json.ContainsKey("tagIds"));
            Assert.False(json.ContainsKey("secretMetadata"));
        }

        [Fact]
        public void BatchUpdateItem_Omits_Null_Optional_Fields()
        {
            object item = MakeDto("PSInfisicalAPI.Secrets.InfisicalSecretBatchUpdateItemDto");
            item.GetType().GetProperty("SecretKey").SetValue(item, "API_KEY");
            item.GetType().GetProperty("NewSecretName").SetValue(item, "RENAMED");

            JObject json = JObject.Parse(JsonConvert.SerializeObject(item));
            Assert.Equal("API_KEY", (string)json["secretKey"]);
            Assert.Equal("RENAMED", (string)json["newSecretName"]);
            Assert.False(json.ContainsKey("secretValue"));
            Assert.False(json.ContainsKey("secretComment"));
        }

        [Fact]
        public void BatchCreateRequest_Serializes_With_Expected_Envelope()
        {
            System.Type itemType = ModuleAssembly.GetType("PSInfisicalAPI.Secrets.InfisicalSecretBatchCreateItemDto", true);
            object item = System.Activator.CreateInstance(itemType);
            itemType.GetProperty("SecretKey").SetValue(item, "K1");
            itemType.GetProperty("SecretValue").SetValue(item, "V1");

            System.Type listType = typeof(List<>).MakeGenericType(itemType);
            object list = System.Activator.CreateInstance(listType);
            listType.GetMethod("Add").Invoke(list, new object[] { item });

            object dto = MakeDto("PSInfisicalAPI.Secrets.InfisicalSecretBatchCreateRequestDto");
            dto.GetType().GetProperty("WorkspaceId").SetValue(dto, "wks-1");
            dto.GetType().GetProperty("ProjectId").SetValue(dto, "wks-1");
            dto.GetType().GetProperty("Environment").SetValue(dto, "prod");
            dto.GetType().GetProperty("SecretPath").SetValue(dto, "/db");
            dto.GetType().GetProperty("Secrets").SetValue(dto, list);

            JObject json = JObject.Parse(JsonConvert.SerializeObject(dto));
            Assert.Equal("wks-1", (string)json["workspaceId"]);
            Assert.Equal("wks-1", (string)json["projectId"]);
            Assert.Equal("prod", (string)json["environment"]);
            Assert.Equal("/db", (string)json["secretPath"]);
            JArray secrets = (JArray)json["secrets"];
            Assert.Single(secrets);
            Assert.Equal("K1", (string)secrets[0]["secretKey"]);
            Assert.Equal("V1", (string)secrets[0]["secretValue"]);
        }

        [Fact]
        public void BatchCreateRequest_Omits_Null_WorkspaceId_When_Only_ProjectId_Set()
        {
            object dto = MakeDto("PSInfisicalAPI.Secrets.InfisicalSecretBatchCreateRequestDto");
            dto.GetType().GetProperty("ProjectId").SetValue(dto, "wks-1");
            dto.GetType().GetProperty("Environment").SetValue(dto, "prod");

            JObject json = JObject.Parse(JsonConvert.SerializeObject(dto));
            Assert.False(json.ContainsKey("workspaceId"));
            Assert.Equal("wks-1", (string)json["projectId"]);
        }

        [Fact]
        public void BatchUpdateRequest_Includes_ProjectId_Alongside_WorkspaceId()
        {
            object dto = MakeDto("PSInfisicalAPI.Secrets.InfisicalSecretBatchUpdateRequestDto");
            dto.GetType().GetProperty("WorkspaceId").SetValue(dto, "wks-1");
            dto.GetType().GetProperty("ProjectId").SetValue(dto, "wks-1");
            dto.GetType().GetProperty("Environment").SetValue(dto, "prod");

            JObject json = JObject.Parse(JsonConvert.SerializeObject(dto));
            Assert.Equal("wks-1", (string)json["workspaceId"]);
            Assert.Equal("wks-1", (string)json["projectId"]);
        }

        [Fact]
        public void BatchDeleteRequest_Serializes_With_Secret_Keys()
        {
            System.Type itemType = ModuleAssembly.GetType("PSInfisicalAPI.Secrets.InfisicalSecretBatchDeleteItemDto", true);
            object item = System.Activator.CreateInstance(itemType);
            itemType.GetProperty("SecretKey").SetValue(item, "K1");

            System.Type listType = typeof(List<>).MakeGenericType(itemType);
            object list = System.Activator.CreateInstance(listType);
            listType.GetMethod("Add").Invoke(list, new object[] { item });

            object dto = MakeDto("PSInfisicalAPI.Secrets.InfisicalSecretBatchDeleteRequestDto");
            dto.GetType().GetProperty("WorkspaceId").SetValue(dto, "wks-1");
            dto.GetType().GetProperty("ProjectId").SetValue(dto, "wks-1");
            dto.GetType().GetProperty("Environment").SetValue(dto, "prod");
            dto.GetType().GetProperty("Secrets").SetValue(dto, list);

            JObject json = JObject.Parse(JsonConvert.SerializeObject(dto));
            Assert.Equal("wks-1", (string)json["workspaceId"]);
            Assert.Equal("wks-1", (string)json["projectId"]);
            JArray secrets = (JArray)json["secrets"];
            Assert.Single(secrets);
            Assert.Equal("K1", (string)secrets[0]["secretKey"]);
        }

        [Fact]
        public void DuplicateRequest_Serializes_With_Expected_Field_Names()
        {
            object dto = MakeDto("PSInfisicalAPI.Secrets.InfisicalSecretDuplicateRequestDto");
            dto.GetType().GetProperty("ProjectId").SetValue(dto, "wks-1");
            dto.GetType().GetProperty("SourceEnvironment").SetValue(dto, "dev");
            dto.GetType().GetProperty("DestinationEnvironment").SetValue(dto, "prod");
            dto.GetType().GetProperty("SourceSecretPath").SetValue(dto, "/db");
            dto.GetType().GetProperty("DestinationSecretPath").SetValue(dto, "/db");
            dto.GetType().GetProperty("SecretIds").SetValue(dto, new[] { "id-1", "id-2" });
            dto.GetType().GetProperty("OverwriteExisting").SetValue(dto, true);

            JObject json = JObject.Parse(JsonConvert.SerializeObject(dto));
            Assert.Equal("wks-1", (string)json["projectId"]);
            Assert.Equal("dev", (string)json["sourceEnvironment"]);
            Assert.Equal("prod", (string)json["destinationEnvironment"]);
            Assert.Equal("/db", (string)json["sourceSecretPath"]);
            Assert.Equal("/db", (string)json["destinationSecretPath"]);
            Assert.True((bool)json["overwriteExisting"]);
            JArray ids = (JArray)json["secretIds"];
            Assert.Equal(2, ids.Count);
            Assert.Equal("id-1", (string)ids[0]);
            Assert.False(json.ContainsKey("attributesToCopy"));
        }

        [Fact]
        public void DuplicateAttributes_Omits_Null_Toggles()
        {
            object attrs = MakeDto("PSInfisicalAPI.Secrets.InfisicalSecretDuplicateAttributesDto");
            attrs.GetType().GetProperty("SecretValue").SetValue(attrs, true);

            JObject json = JObject.Parse(JsonConvert.SerializeObject(attrs));
            Assert.True((bool)json["secretValue"]);
            Assert.False(json.ContainsKey("secretComment"));
            Assert.False(json.ContainsKey("tags"));
            Assert.False(json.ContainsKey("metadata"));
        }
    }
}
