using System.IO;
using System.Reflection;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class ManifestTests
    {
        private static DirectoryInfo RepositoryRoot()
        {
            DirectoryInfo current = new DirectoryInfo(System.AppContext.BaseDirectory);
            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, "PSInfisicalAPI.sln")))
                {
                    return current;
                }

                current = current.Parent;
            }

            return null;
        }

        [Fact]
        public void Assembly_Has_CommitHash_Metadata()
        {
            AssemblyMetadataAttribute[] attributes = (AssemblyMetadataAttribute[])typeof(PSInfisicalAPI.Connections.InfisicalConnection)
                .Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false);

            bool found = false;
            foreach (AssemblyMetadataAttribute attribute in attributes)
            {
                if (attribute.Key == "CommitHash")
                {
                    found = true;
                    Assert.False(string.IsNullOrEmpty(attribute.Value));
                }
            }

            Assert.True(found, "Assembly must contain a CommitHash metadata attribute.");
        }

        [Fact]
        public void Psm1_Imports_Binary_From_Bin_Folder()
        {
            DirectoryInfo root = RepositoryRoot();
            Assert.NotNull(root);
            string psm1Path = Path.Combine(root.FullName, "Module", "PSInfisicalAPI", "PSInfisicalAPI.psm1");
            Assert.True(File.Exists(psm1Path));

            string content = File.ReadAllText(psm1Path);
            Assert.Contains("Import-Module", content);
            Assert.Contains("'bin'", content);
            Assert.Contains("PSInfisicalAPI.dll", content);
        }
    }
}
