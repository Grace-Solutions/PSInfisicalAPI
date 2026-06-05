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
        public void GetInfisicalCertificate_Cmdlet_Is_Singular_With_SerialNumber_In_Single_ParameterSet()
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

            string defaultParameterSetName = null;
            foreach (CustomAttributeNamedArgument named in cmdletData.NamedArguments)
            {
                if (named.MemberName == "DefaultParameterSetName") { defaultParameterSetName = (string)named.TypedValue.Value; break; }
            }
            Assert.Equal("List", defaultParameterSetName);

            PropertyInfo serialProp = cmdletType.GetProperty("SerialNumber");
            Assert.NotNull(serialProp);

            CustomAttributeData parameterAttr = null;
            foreach (CustomAttributeData candidate in serialProp.GetCustomAttributesData())
            {
                if (candidate.AttributeType == typeof(ParameterAttribute)) { parameterAttr = candidate; break; }
            }
            Assert.NotNull(parameterAttr);

            bool mandatory = false;
            string parameterSetName = null;
            foreach (CustomAttributeNamedArgument named in parameterAttr.NamedArguments)
            {
                if (named.MemberName == "Mandatory") { mandatory = (bool)named.TypedValue.Value; }
                else if (named.MemberName == "ParameterSetName") { parameterSetName = (string)named.TypedValue.Value; }
            }
            Assert.True(mandatory);
            Assert.Equal("Single", parameterSetName);
        }

        [Fact]
        public void GetInfisicalCertificate_Cmdlet_Exposes_List_Filter_Properties()
        {
            Type cmdletType = ModuleAssembly.GetType("PSInfisicalAPI.Cmdlets.GetInfisicalCertificateCmdlet", true);
            Assert.NotNull(cmdletType.GetProperty("CommonName"));
            Assert.NotNull(cmdletType.GetProperty("FriendlyName"));
            Assert.NotNull(cmdletType.GetProperty("Search"));
            Assert.NotNull(cmdletType.GetProperty("Status"));
            Assert.NotNull(cmdletType.GetProperty("CaId"));
            Assert.NotNull(cmdletType.GetProperty("ProfileId"));
            Assert.NotNull(cmdletType.GetProperty("ApplicationId"));
            Assert.NotNull(cmdletType.GetProperty("ApplicationIds"));
            Assert.NotNull(cmdletType.GetProperty("EnrollmentType"));
            Assert.NotNull(cmdletType.GetProperty("ExtendedKeyUsage"));
            Assert.NotNull(cmdletType.GetProperty("KeyAlgorithm"));
            Assert.NotNull(cmdletType.GetProperty("SignatureAlgorithm"));
            Assert.NotNull(cmdletType.GetProperty("KeySize"));
            Assert.NotNull(cmdletType.GetProperty("Source"));
            Assert.NotNull(cmdletType.GetProperty("FromDate"));
            Assert.NotNull(cmdletType.GetProperty("ToDate"));
            Assert.NotNull(cmdletType.GetProperty("NotAfterFrom"));
            Assert.NotNull(cmdletType.GetProperty("NotAfterTo"));
            Assert.NotNull(cmdletType.GetProperty("NotBeforeFrom"));
            Assert.NotNull(cmdletType.GetProperty("NotBeforeTo"));
            Assert.NotNull(cmdletType.GetProperty("Metadata"));
            Assert.NotNull(cmdletType.GetProperty("ForPkiSync"));
            Assert.NotNull(cmdletType.GetProperty("SortBy"));
            Assert.NotNull(cmdletType.GetProperty("SortOrder"));
            Assert.NotNull(cmdletType.GetProperty("Limit"));
            Assert.NotNull(cmdletType.GetProperty("Offset"));
            Assert.NotNull(cmdletType.GetProperty("NoAutoPage"));
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
