using System.Reflection;
using PSInfisicalAPI.Models;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class FolderMapperTests
    {
        private static readonly System.Type MapperType = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly
            .GetType("PSInfisicalAPI.Folders.InfisicalFolderMapper", true);

        private static readonly System.Type DtoType = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly
            .GetType("PSInfisicalAPI.Folders.InfisicalFolderResponseDto", true);

        private static InfisicalFolder InvokeMap(object dto, string fallbackProjectId, string fallbackEnvironment)
        {
            MethodInfo map = MapperType.GetMethod("Map", BindingFlags.Public | BindingFlags.Static);
            return (InfisicalFolder)map.Invoke(null, new object[] { dto, fallbackProjectId, fallbackEnvironment });
        }

        [Fact]
        public void Map_Null_Returns_Null()
        {
            Assert.Null(InvokeMap(null, "proj-x", "dev"));
        }

        [Fact]
        public void Map_Populates_Fields_With_Explicit_ProjectId()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("Id").SetValue(dto, "fld-001");
            DtoType.GetProperty("Name").SetValue(dto, "config");
            DtoType.GetProperty("Path").SetValue(dto, "/app/config");
            DtoType.GetProperty("ParentId").SetValue(dto, "fld-root");
            DtoType.GetProperty("Environment").SetValue(dto, "prod");
            DtoType.GetProperty("ProjectId").SetValue(dto, "proj-001");

            InfisicalFolder folder = InvokeMap(dto, "fallback-proj", "fallback-env");

            Assert.Equal("fld-001", folder.Id);
            Assert.Equal("config", folder.Name);
            Assert.Equal("/app/config", folder.Path);
            Assert.Equal("fld-root", folder.ParentId);
            Assert.Equal("prod", folder.Environment);
            Assert.Equal("proj-001", folder.ProjectId);
        }

        [Fact]
        public void Map_Uses_WorkspaceId_When_ProjectId_Empty()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("Id").SetValue(dto, "fld-002");
            DtoType.GetProperty("WorkspaceId").SetValue(dto, "wks-002");

            InfisicalFolder folder = InvokeMap(dto, "fallback-proj", "fallback-env");
            Assert.Equal("wks-002", folder.ProjectId);
        }

        [Fact]
        public void Map_Uses_Fallback_When_No_ProjectId_Or_Environment()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("Id").SetValue(dto, "fld-003");

            InfisicalFolder folder = InvokeMap(dto, "fallback-proj", "fallback-env");
            Assert.Equal("fallback-proj", folder.ProjectId);
            Assert.Equal("fallback-env", folder.Environment);
        }

        [Fact]
        public void Map_Falls_Back_To_InternalId_For_Id()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("InternalId").SetValue(dto, "internal-fld");

            InfisicalFolder folder = InvokeMap(dto, "p", "e");
            Assert.Equal("internal-fld", folder.Id);
        }
    }
}
