using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using PSInfisicalAPI.Endpoints;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class PkiEndpointRegistryTests
    {
        private static readonly Assembly ModuleAssembly = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly;

        [Fact]
        public void GetInfisicalCertificate_Cmdlet_Is_Singular_With_Mandatory_SerialNumber()
        {
            Type cmdletType = ModuleAssembly.GetType("PSInfisicalAPI.Cmdlets.GetInfisicalCertificateCmdlet", true);
            Assert.True(typeof(PSInfisicalAPI.Cmdlets.InfisicalCmdletBase).IsAssignableFrom(cmdletType));

            CustomAttributeData cmdletData = null;
            foreach (CustomAttributeData candidate in cmdletType.GetCustomAttributesData())
            {
                if (candidate.AttributeType == typeof(CmdletAttribute)) { cmdletData = candidate; break; }
            }
            Assert.NotNull(cmdletData);
            Assert.Equal(2, cmdletData.ConstructorArguments.Count);
            Assert.Equal(VerbsCommon.Get, cmdletData.ConstructorArguments[0].Value);
            Assert.Equal("InfisicalCertificate", cmdletData.ConstructorArguments[1].Value);

            PropertyInfo serialProp = cmdletType.GetProperty("SerialNumber");
            Assert.NotNull(serialProp);

            CustomAttributeData parameterAttr = null;
            foreach (CustomAttributeData candidate in serialProp.GetCustomAttributesData())
            {
                if (candidate.AttributeType == typeof(ParameterAttribute)) { parameterAttr = candidate; break; }
            }
            Assert.NotNull(parameterAttr);

            bool mandatory = false;
            foreach (CustomAttributeNamedArgument named in parameterAttr.NamedArguments)
            {
                if (named.MemberName == "Mandatory") { mandatory = (bool)named.TypedValue.Value; break; }
            }
            Assert.True(mandatory);
        }

        [Fact]
        public void GetInfisicalCertificates_Cmdlet_Is_Registered_For_Listing()
        {
            Type cmdletType = ModuleAssembly.GetType("PSInfisicalAPI.Cmdlets.GetInfisicalCertificatesCmdlet", true);
            Assert.True(typeof(PSInfisicalAPI.Cmdlets.InfisicalCmdletBase).IsAssignableFrom(cmdletType));

            CustomAttributeData cmdletData = null;
            foreach (CustomAttributeData candidate in cmdletType.GetCustomAttributesData())
            {
                if (candidate.AttributeType == typeof(CmdletAttribute)) { cmdletData = candidate; break; }
            }
            Assert.NotNull(cmdletData);
            Assert.Equal(VerbsCommon.Get, cmdletData.ConstructorArguments[0].Value);
            Assert.Equal("InfisicalCertificates", cmdletData.ConstructorArguments[1].Value);

            Assert.NotNull(cmdletType.GetProperty("CommonName"));
            Assert.NotNull(cmdletType.GetProperty("FriendlyName"));
            Assert.NotNull(cmdletType.GetProperty("CaId"));
            Assert.NotNull(cmdletType.GetProperty("Limit"));
            Assert.NotNull(cmdletType.GetProperty("Offset"));
        }

        [Fact]
        public void Get_ListInternalCertificateAuthorities_Returns_CertManager_Primary()
        {
            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(InfisicalEndpointNames.ListInternalCertificateAuthorities);
            Assert.Equal("GET", definition.Method);
            Assert.Equal("v1", definition.Version);
            Assert.Equal("/api/v1/cert-manager/ca/internal", definition.Template);
            Assert.True(definition.RequiresAuthorization);
        }

        [Fact]
        public void Get_RetrieveInternalCertificateAuthority_Has_CaId_Placeholder_Under_CertManager()
        {
            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(InfisicalEndpointNames.RetrieveInternalCertificateAuthority);
            Assert.Equal("GET", definition.Method);
            Assert.Equal("/api/v1/cert-manager/ca/internal/{caId}", definition.Template);
        }

        [Fact]
        public void Get_SearchCertificates_Is_Post_With_ProjectId_Placeholder()
        {
            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(InfisicalEndpointNames.SearchCertificates);
            Assert.Equal("POST", definition.Method);
            Assert.Equal("/api/v1/projects/{projectId}/certificates/search", definition.Template);
            Assert.True(definition.RequiresAuthorization);
        }

        [Fact]
        public void Get_GetCertificateBundle_Marks_Response_As_Secret_With_Pki_Primary()
        {
            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(InfisicalEndpointNames.GetCertificateBundle);
            Assert.Equal("GET", definition.Method);
            Assert.Equal("/api/v1/pki/certificates/{serialNumber}/bundle", definition.Template);
            Assert.True(definition.ContainsSecretMaterialInResponse);
            Assert.True(definition.RequiresAuthorization);
        }

        [Fact]
        public void Candidates_For_ListInternalCertificateAuthorities_Include_Pki_Legacy_Alias()
        {
            IReadOnlyList<InfisicalEndpointDefinition> candidates = InfisicalEndpointRegistry.GetCandidates(InfisicalEndpointNames.ListInternalCertificateAuthorities);
            Assert.Contains(candidates, c => c.Template == "/api/v1/cert-manager/ca/internal");
            Assert.Contains(candidates, c => c.Template == "/api/v1/pki/ca/internal");
        }

        [Fact]
        public void Candidates_For_RetrieveInternalCertificateAuthority_Include_Pki_Legacy_Alias()
        {
            IReadOnlyList<InfisicalEndpointDefinition> candidates = InfisicalEndpointRegistry.GetCandidates(InfisicalEndpointNames.RetrieveInternalCertificateAuthority);
            Assert.Contains(candidates, c => c.Template == "/api/v1/cert-manager/ca/internal/{caId}");
            Assert.Contains(candidates, c => c.Template == "/api/v1/pki/ca/internal/{caId}");
        }

        [Fact]
        public void Candidates_For_GetCertificateBundle_Include_CertManager_Alias()
        {
            IReadOnlyList<InfisicalEndpointDefinition> candidates = InfisicalEndpointRegistry.GetCandidates(InfisicalEndpointNames.GetCertificateBundle);
            Assert.Contains(candidates, c => c.Template == "/api/v1/pki/certificates/{serialNumber}/bundle");
            Assert.Contains(candidates, c => c.Template == "/api/v1/cert-manager/certificates/{serialNumber}/bundle");
        }
    }
}
