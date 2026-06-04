using System;

namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalCertificateApplication
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ProfileCount { get; set; }
        public int? MemberCount { get; set; }
        public int? CertificateCount { get; set; }
        public DateTimeOffset? CreatedAtUtc { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }
    }

    public sealed class InfisicalCertificateApplicationProfileAttachment
    {
        public string ApplicationId { get; set; }
        public string ProfileId { get; set; }
        public string ProfileSlug { get; set; }
        public string ProfileDescription { get; set; }
        public string ApiConfigId { get; set; }
        public string EstConfigId { get; set; }
        public string AcmeConfigId { get; set; }
        public string ScepConfigId { get; set; }
        public DateTimeOffset? CreatedAtUtc { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }
    }
}
