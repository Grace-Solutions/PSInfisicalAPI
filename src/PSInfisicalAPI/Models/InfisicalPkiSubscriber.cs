using System;

namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalPkiSubscriber
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string CaId { get; set; }
        public string Name { get; set; }
        public string CommonName { get; set; }
        public string Status { get; set; }
        public string Ttl { get; set; }
        public string[] SubjectAlternativeNames { get; set; }
        public string[] KeyUsages { get; set; }
        public string[] ExtendedKeyUsages { get; set; }
        public bool? EnableAutoRenewal { get; set; }
        public int? AutoRenewalPeriodInDays { get; set; }
        public string LastOperationStatus { get; set; }
        public string LastOperationMessage { get; set; }
        public DateTimeOffset? LastOperationAtUtc { get; set; }
        public DateTimeOffset? CreatedAtUtc { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }
        public InfisicalPkiSubscriberProperties Properties { get; set; }
    }

    public sealed class InfisicalPkiSubscriberProperties
    {
        public string AzureTemplateType { get; set; }
        public string Organization { get; set; }
        public string OrganizationalUnit { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Locality { get; set; }
        public string EmailAddress { get; set; }
    }
}
