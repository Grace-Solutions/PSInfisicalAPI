using System;

namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalSubOrganization
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string OrganizationId { get; set; }
        public bool IsAccessible { get; set; }
        public DateTimeOffset? CreatedAtUtc { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Slug) ? (Name ?? Id) : Slug;
        }
    }
}
