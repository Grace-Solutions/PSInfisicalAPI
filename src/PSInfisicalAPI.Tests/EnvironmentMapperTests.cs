using System.Reflection;
using PSInfisicalAPI.Models;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class EnvironmentMapperTests
    {
        private static readonly System.Type MapperType = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly
            .GetType("PSInfisicalAPI.Environments.InfisicalEnvironmentMapper", true);

        private static readonly System.Type DtoType = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly
            .GetType("PSInfisicalAPI.Environments.InfisicalEnvironmentResponseDto", true);

        private static InfisicalEnvironment InvokeMap(object dto, string fallbackProjectId)
        {
            MethodInfo map = MapperType.GetMethod("Map", BindingFlags.Public | BindingFlags.Static);
            return (InfisicalEnvironment)map.Invoke(null, new object[] { dto, fallbackProjectId });
        }

        [Fact]
        public void Map_Null_Returns_Null()
        {
            Assert.Null(InvokeMap(null, "proj-x"));
        }

        [Fact]
        public void Map_Populates_Fields_With_Explicit_ProjectId()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("Id").SetValue(dto, "env-001");
            DtoType.GetProperty("Name").SetValue(dto, "Production");
            DtoType.GetProperty("Slug").SetValue(dto, "prod");
            DtoType.GetProperty("Position").SetValue(dto, 1);
            DtoType.GetProperty("ProjectId").SetValue(dto, "proj-001");

            InfisicalEnvironment env = InvokeMap(dto, "fallback-proj");

            Assert.Equal("env-001", env.Id);
            Assert.Equal("Production", env.Name);
            Assert.Equal("prod", env.Slug);
            Assert.Equal(1, env.Position);
            Assert.Equal("proj-001", env.ProjectId);
        }

        [Fact]
        public void Map_Uses_WorkspaceId_When_ProjectId_Empty()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("Id").SetValue(dto, "env-002");
            DtoType.GetProperty("WorkspaceId").SetValue(dto, "wks-002");

            InfisicalEnvironment env = InvokeMap(dto, "fallback-proj");
            Assert.Equal("wks-002", env.ProjectId);
        }

        [Fact]
        public void Map_Uses_Fallback_When_No_ProjectId_Or_WorkspaceId()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("Id").SetValue(dto, "env-003");

            InfisicalEnvironment env = InvokeMap(dto, "fallback-proj");
            Assert.Equal("fallback-proj", env.ProjectId);
        }

        [Fact]
        public void Map_Falls_Back_To_InternalId_For_Id()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("InternalId").SetValue(dto, "internal-env");

            InfisicalEnvironment env = InvokeMap(dto, "p");
            Assert.Equal("internal-env", env.Id);
        }
    }
}
