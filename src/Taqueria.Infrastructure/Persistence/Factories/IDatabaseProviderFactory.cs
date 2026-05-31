using Microsoft.EntityFrameworkCore;

namespace Taqueria.Infrastructure.Persistence.Factories;

public interface IDatabaseProviderFactory
{
    string ProviderName { get; }
    void Configure(DbContextOptionsBuilder optionsBuilder, DatabaseOptions databaseOptions);
}
