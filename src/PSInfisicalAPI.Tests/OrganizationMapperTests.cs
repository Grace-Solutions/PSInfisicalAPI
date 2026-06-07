using System.Reflection;
using PSInfisicalAPI.Models;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class OrganizationMapperTests
    {
        private static readonly System.Type MapperType = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly
            .GetType("PSInfisicalAPI.Organizations.InfisicalOrganizationMapper", true);

        private static readonly System.Type DtoType = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly
            .GetType("PSInfisicalAPI.Organizations.InfisicalOrganizationResponseDto", true);

        private static InfisicalOrganization InvokeMap(object dto)
        {
            MethodInfo map = MapperType.GetMethod("Map", BindingFlags.Public | BindingFlags.Static);
            return (InfisicalOrganization)map.Invoke(null, new[] { dto });
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
            DtoType.GetProperty("Id").SetValue(dto, "org-001");
            DtoType.GetProperty("Name").SetValue(dto, "Acme");
            DtoType.GetProperty("Slug").SetValue(dto, "acme");
            DtoType.GetProperty("CustomerId").SetValue(dto, "cust-9");
            DtoType.GetProperty("AuthEnforced").SetValue(dto, true);
            DtoType.GetProperty("ScimEnabled").SetValue(dto, true);
            DtoType.GetProperty("CreatedAt").SetValue(dto, "2026-01-15T12:34:56Z");
            DtoType.GetProperty("UpdatedAt").SetValue(dto, "2026-02-20T09:00:00Z");

            InfisicalOrganization organization = InvokeMap(dto);

            Assert.Equal("org-001", organization.Id);
            Assert.Equal("Acme", organization.Name);
            Assert.Equal("acme", organization.Slug);
            Assert.Equal("cust-9", organization.CustomerId);
            Assert.True(organization.AuthEnforced);
            Assert.True(organization.ScimEnabled);
            Assert.NotNull(organization.CreatedAtUtc);
            Assert.NotNull(organization.UpdatedAtUtc);
        }

        [Fact]
        public void Map_Falls_Back_To_InternalId()
        {
            object dto = System.Activator.CreateInstance(DtoType);
            DtoType.GetProperty("InternalId").SetValue(dto, "internal-id-1");

            InfisicalOrganization organization = InvokeMap(dto);

            Assert.Equal("internal-id-1", organization.Id);
        }

        [Fact]
        public void MapMany_Null_Returns_Empty()
        {
            MethodInfo mapMany = MapperType.GetMethod("MapMany", BindingFlags.Public | BindingFlags.Static);
            InfisicalOrganization[] result = (InfisicalOrganization[])mapMany.Invoke(null, new object[] { null });
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
