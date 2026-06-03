namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalEnvironment
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public int? Position { get; set; }
        public string ProjectId { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Slug) ? (Name ?? Id) : Slug;
        }
    }
}
