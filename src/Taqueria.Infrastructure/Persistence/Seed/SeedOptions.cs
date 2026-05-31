namespace Taqueria.Infrastructure.Persistence.Seed;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";
    public bool LoadDemoCatalog { get; set; } = true;
}
