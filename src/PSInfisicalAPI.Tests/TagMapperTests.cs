using System.Reflection;
using PSInfisicalAPI.Models;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class TagMapperTests
    {
        private static readonly System.Type MapperType = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly
            .GetType("PSInfisicalAPI.Tags.InfisicalTagMapper", true);

        private static readonly System.Type DtoType = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly
            .GetType("PSInfisicalAPI.Tags.InfisicalTagResponseDto", true);

        private static InfisicalTag InvokeMap(object dto, string fallbackProjectId)
        {
            MethodInfo map = MapperType.GetMethod("Map", BindingFlags.Public | BindingFlags.Static);
            return (InfisicalTag)map.Invoke(null, new object[] { dto, fallbackProjectId });
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
            DtoType.GetProperty("Id").SetValue(dto, "tag-001");
            DtoType.GetProperty("Slug").SetValue(dto, "critical");
            DtoType.GetProperty("Name").SetValue(dto, "Critical");
            DtoType.GetProperty("Color").SetValue(dto, "#FF0000");
            DtoType.GetProperty("ProjectId").SetValue(dto, "proj-001");

            InfisicalTag tag = InvokeMap(dto, "fallback-proj");

            Assert.Equal("tag-001", tag.Id);
            Assert.Equal("critical", tag.Slug);
            Assert.Equal("Critical", tag.Name);
            Assert.Equal("#FF0000", tag.Color);
            Assert.Equal("proj-001", tag.ProjectId);
        }

        [Fact]
        public void Map_Uses_WorkspaceId_When_ProjectId_Empty()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("Id").SetValue(dto, "tag-002");
            DtoType.GetProperty("WorkspaceId").SetValue(dto, "wks-002");

            InfisicalTag tag = InvokeMap(dto, "fallback-proj");
            Assert.Equal("wks-002", tag.ProjectId);
        }

        [Fact]
        public void Map_Uses_Fallback_When_No_ProjectId_Or_WorkspaceId()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("Id").SetValue(dto, "tag-003");

            InfisicalTag tag = InvokeMap(dto, "fallback-proj");
            Assert.Equal("fallback-proj", tag.ProjectId);
        }

        [Fact]
        public void Map_Falls_Back_To_InternalId_For_Id()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("InternalId").SetValue(dto, "internal-tag");

            InfisicalTag tag = InvokeMap(dto, "p");
            Assert.Equal("internal-tag", tag.Id);
        }
    }
}
