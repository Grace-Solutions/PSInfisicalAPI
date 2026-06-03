using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class SecretMutationDtoTests
    {
        private static readonly System.Reflection.Assembly ModuleAssembly =
            typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly;

        private static object MakeDto(string typeName)
        {
            System.Type t = ModuleAssembly.GetType(typeName, true);
            return System.Activator.CreateInstance(t);
        }

        [Fact]
        public void CreateRequestDto_Serializes_With_Expected_Field_Names()
        {
            object dto = MakeDto("PSInfisicalAPI.Secrets.InfisicalSecretCreateRequestDto");
            dto.GetType().GetProperty("WorkspaceId").SetValue(dto, "wks-1");
            dto.GetType().GetProperty("Environment").SetValue(dto, "prod");
            dto.GetType().GetProperty("SecretPath").SetValue(dto, "/db");
            dto.GetType().GetProperty("Type").SetValue(dto, "shared");
            dto.GetType().GetProperty("SecretValue").SetValue(dto, "p@ss");
            dto.GetType().GetProperty("SecretComment").SetValue(dto, "comment");

            JObject json = JObject.Parse(JsonConvert.SerializeObject(dto));
            Assert.Equal("wks-1", (string)json["workspaceId"]);
            Assert.Equal("prod", (string)json["environment"]);
            Assert.Equal("/db", (string)json["secretPath"]);
            Assert.Equal("shared", (string)json["type"]);
            Assert.Equal("p@ss", (string)json["secretValue"]);
            Assert.Equal("comment", (string)json["secretComment"]);
            Assert.False(json.ContainsKey("skipMultilineEncoding"));
            Assert.False(json.ContainsKey("tagIds"));
        }

        [Fact]
        public void UpdateRequestDto_Omits_Null_Optional_Fields()
        {
            object dto = MakeDto("PSInfisicalAPI.Secrets.InfisicalSecretUpdateRequestDto");
            dto.GetType().GetProperty("WorkspaceId").SetValue(dto, "wks-1");
            dto.GetType().GetProperty("Environment").SetValue(dto, "prod");
            dto.GetType().GetProperty("NewSecretName").SetValue(dto, "renamed");

            JObject json = JObject.Parse(JsonConvert.SerializeObject(dto));
            Assert.Equal("renamed", (string)json["newSecretName"]);
            Assert.False(json.ContainsKey("secretValue"));
            Assert.False(json.ContainsKey("secretComment"));
            Assert.False(json.ContainsKey("type"));
            Assert.False(json.ContainsKey("secretPath"));
        }

        [Fact]
        public void DeleteRequestDto_Serializes_With_Expected_Field_Names()
        {
            object dto = MakeDto("PSInfisicalAPI.Secrets.InfisicalSecretDeleteRequestDto");
            dto.GetType().GetProperty("WorkspaceId").SetValue(dto, "wks-1");
            dto.GetType().GetProperty("Environment").SetValue(dto, "prod");
            dto.GetType().GetProperty("SecretPath").SetValue(dto, "/db");
            dto.GetType().GetProperty("Type").SetValue(dto, "shared");

            JObject json = JObject.Parse(JsonConvert.SerializeObject(dto));
            Assert.Equal("wks-1", (string)json["workspaceId"]);
            Assert.Equal("prod", (string)json["environment"]);
            Assert.Equal("/db", (string)json["secretPath"]);
            Assert.Equal("shared", (string)json["type"]);
        }
    }
}
