using System;

namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalFolder
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string ParentId { get; set; }
        public string Environment { get; set; }
        public string ProjectId { get; set; }
        public DateTimeOffset? CreatedAtUtc { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Path) ? (Name ?? Id) : Path;
        }
    }
}
