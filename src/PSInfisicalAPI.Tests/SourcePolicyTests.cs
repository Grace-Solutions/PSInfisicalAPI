using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class SourcePolicyTests
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
        public void No_Async_Or_Await_Keywords_In_Production_Source()
        {
            DirectoryInfo root = RepositoryRoot();
            Assert.NotNull(root);
            DirectoryInfo src = new DirectoryInfo(Path.Combine(root.FullName, "src", "PSInfisicalAPI"));
            Assert.True(src.Exists);

            Regex asyncRegex = new Regex(@"\basync\b");
            Regex awaitRegex = new Regex(@"\bawait\b");

            foreach (FileInfo file in src.EnumerateFiles("*.cs", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(file.FullName);
                Assert.DoesNotMatch(asyncRegex, content);
                Assert.DoesNotMatch(awaitRegex, content);
            }
        }
    }
}
