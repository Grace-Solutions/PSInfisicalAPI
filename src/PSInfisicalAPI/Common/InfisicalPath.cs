using System.IO;

namespace PSInfisicalAPI.Common
{
    public static class InfisicalPath
    {
        public static FileInfo CombineFile(params string[] segments)
        {
            string combined = Path.Combine(segments);
            return new FileInfo(combined);
        }

        public static DirectoryInfo CombineDirectory(params string[] segments)
        {
            string combined = Path.Combine(segments);
            return new DirectoryInfo(combined);
        }

        public static void EnsureDirectoryExists(DirectoryInfo directory)
        {
            if (directory == null)
            {
                return;
            }

            if (!directory.Exists)
            {
                directory.Create();
            }
        }

        public static void EnsureParentDirectoryExists(FileInfo file)
        {
            if (file == null)
            {
                return;
            }

            EnsureDirectoryExists(file.Directory);
        }
    }
}
