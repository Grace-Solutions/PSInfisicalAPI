using System.Collections.Generic;
using System.IO;

namespace PSInfisicalAPI.Imports
{
    public interface IInfisicalImporter
    {
        IList<KeyValuePair<string, string>> Import(FileInfo path);
    }
}
