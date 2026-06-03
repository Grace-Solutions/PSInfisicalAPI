using System;

namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalTag
    {
        public string Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string ProjectId { get; set; }
        public DateTimeOffset? CreatedAtUtc { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }

        public override string ToString()
        {
            return Slug ?? Name ?? Id;
        }
    }
}
