using System.Collections;
using System.Reflection;
using PSInfisicalAPI.Models;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class ProjectMapperTests
    {
        private static readonly System.Type MapperType = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly
            .GetType("PSInfisicalAPI.Projects.InfisicalProjectMapper", true);

        private static readonly System.Type DtoType = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly
            .GetType("PSInfisicalAPI.Projects.InfisicalProjectResponseDto", true);

        private static readonly System.Type EnvDtoType = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly
            .GetType("PSInfisicalAPI.Projects.InfisicalProjectEnvironmentDto", true);

        private static InfisicalProject InvokeMap(object dto)
        {
            MethodInfo map = MapperType.GetMethod("Map", BindingFlags.Public | BindingFlags.Static);
            return (InfisicalProject)map.Invoke(null, new[] { dto });
        }

        [Fact]
        public void Map_Null_Dto_Returns_Null()
        {
            Assert.Null(InvokeMap(null));
        }

        [Fact]
        public void Map_Populates_Core_Fields()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("Id").SetValue(dto, "proj-001");
            DtoType.GetProperty("Name").SetValue(dto, "DevOps");
            DtoType.GetProperty("Slug").SetValue(dto, "devops");
            DtoType.GetProperty("Description").SetValue(dto, "Internal DevOps project");
            DtoType.GetProperty("Organization").SetValue(dto, "org-abc");
            DtoType.GetProperty("Type").SetValue(dto, "secret-manager");
            DtoType.GetProperty("AutoCapitalization").SetValue(dto, true);
            DtoType.GetProperty("CreatedAt").SetValue(dto, "2026-01-15T12:34:56Z");
            DtoType.GetProperty("UpdatedAt").SetValue(dto, "2026-02-20T09:00:00Z");

            InfisicalProject project = InvokeMap(dto);

            Assert.Equal("proj-001", project.Id);
            Assert.Equal("DevOps", project.Name);
            Assert.Equal("devops", project.Slug);
            Assert.Equal("Internal DevOps project", project.Description);
            Assert.Equal("org-abc", project.OrganizationId);
            Assert.Equal("secret-manager", project.Type);
            Assert.True(project.AutoCapitalization);
            Assert.NotNull(project.CreatedAtUtc);
            Assert.NotNull(project.UpdatedAtUtc);
            Assert.Empty(project.EnvironmentSlugs);
        }

        [Fact]
        public void Map_Falls_Back_To_InternalId_And_OrgId()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("InternalId").SetValue(dto, "internal-id-1");
            DtoType.GetProperty("OrgId").SetValue(dto, "org-fallback");

            InfisicalProject project = InvokeMap(dto);

            Assert.Equal("internal-id-1", project.Id);
            Assert.Equal("org-fallback", project.OrganizationId);
        }

        [Fact]
        public void Map_Extracts_EnvironmentSlugs()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("Id").SetValue(dto, "proj-002");

            System.Type listType = typeof(System.Collections.Generic.List<>).MakeGenericType(EnvDtoType);
            IList envs = (IList)System.Activator.CreateInstance(listType);

            object env1 = System.Activator.CreateInstance(EnvDtoType);
            EnvDtoType.GetProperty("Slug").SetValue(env1, "dev");
            EnvDtoType.GetProperty("Name").SetValue(env1, "Development");
            envs.Add(env1);

            object env2 = System.Activator.CreateInstance(EnvDtoType);
            EnvDtoType.GetProperty("Slug").SetValue(env2, "prod");
            envs.Add(env2);

            DtoType.GetProperty("Environments").SetValue(dto, envs);

            InfisicalProject project = InvokeMap(dto);

            Assert.Equal(2, project.EnvironmentSlugs.Length);
            Assert.Contains("dev", project.EnvironmentSlugs);
            Assert.Contains("prod", project.EnvironmentSlugs);
        }

        [Fact]
        public void MapMany_Null_Returns_Empty()
        {
            MethodInfo mapMany = MapperType.GetMethod("MapMany", BindingFlags.Public | BindingFlags.Static);
            InfisicalProject[] result = (InfisicalProject[])mapMany.Invoke(null, new object[] { null });
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
