using System.Reflection;
using PSInfisicalAPI.Models;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class SubOrganizationMapperTests
    {
        private static readonly System.Type MapperType = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly
            .GetType("PSInfisicalAPI.SubOrganizations.InfisicalSubOrganizationMapper", true);

        private static readonly System.Type DtoType = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly
            .GetType("PSInfisicalAPI.SubOrganizations.InfisicalSubOrganizationResponseDto", true);

        private static InfisicalSubOrganization InvokeMap(object dto)
        {
            MethodInfo map = MapperType.GetMethod("Map", BindingFlags.Public | BindingFlags.Static);
            return (InfisicalSubOrganization)map.Invoke(null, new[] { dto });
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
            DtoType.GetProperty("Id").SetValue(dto, "sub-001");
            DtoType.GetProperty("Name").SetValue(dto, "Platform Engineering");
            DtoType.GetProperty("Slug").SetValue(dto, "platform-eng");
            DtoType.GetProperty("OrganizationId").SetValue(dto, "org-001");
            DtoType.GetProperty("IsAccessible").SetValue(dto, true);
            DtoType.GetProperty("CreatedAt").SetValue(dto, "2026-01-15T12:34:56Z");
            DtoType.GetProperty("UpdatedAt").SetValue(dto, "2026-02-20T09:00:00Z");

            InfisicalSubOrganization subOrganization = InvokeMap(dto);

            Assert.Equal("sub-001", subOrganization.Id);
            Assert.Equal("Platform Engineering", subOrganization.Name);
            Assert.Equal("platform-eng", subOrganization.Slug);
            Assert.Equal("org-001", subOrganization.OrganizationId);
            Assert.True(subOrganization.IsAccessible);
            Assert.NotNull(subOrganization.CreatedAtUtc);
            Assert.NotNull(subOrganization.UpdatedAtUtc);
        }

        [Fact]
        public void Map_Falls_Back_To_InternalId_And_OrgId()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("InternalId").SetValue(dto, "internal-id-1");
            DtoType.GetProperty("OrgId").SetValue(dto, "org-fallback");

            InfisicalSubOrganization subOrganization = InvokeMap(dto);

            Assert.Equal("internal-id-1", subOrganization.Id);
            Assert.Equal("org-fallback", subOrganization.OrganizationId);
        }

        [Fact]
        public void MapMany_Null_Returns_Empty()
        {
            MethodInfo mapMany = MapperType.GetMethod("MapMany", BindingFlags.Public | BindingFlags.Static);
            InfisicalSubOrganization[] result = (InfisicalSubOrganization[])mapMany.Invoke(null, new object[] { null });
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
