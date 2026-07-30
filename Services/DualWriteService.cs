using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;

namespace PetFeeder.API.Services
{
    public class DualWriteService
    {
        private readonly AppDbContext _primary;
        private readonly IDbContextFactory<AppDbContext>? _secondaryFactory;

        public DualWriteService(AppDbContext primary, IDbContextFactory<AppDbContext>? secondaryFactory = null)
        {
            _primary = primary;
            _secondaryFactory = secondaryFactory;
        }

        public AppDbContext Db => _primary;

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var changes = _primary.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Unchanged && e.State != EntityState.Detached)
                .Select(e => new { e.Entity, e.State })
                .ToList();

            var result = await _primary.SaveChangesAsync(ct);

            if (_secondaryFactory != null && changes.Count > 0)
            {
                using var secondary = await _secondaryFactory.CreateDbContextAsync(ct);
                await secondary.Database.EnsureCreatedAsync(ct);

                foreach (var change in changes)
                {
                    var entry = secondary.Entry(change.Entity);
                    if (change.State == EntityState.Added)
                    {
                        var pk = entry.Metadata.FindPrimaryKey();
                        if (pk?.Properties.Count == 1)
                        {
                            var pkProp = pk.Properties[0];
                            entry.Property(pkProp.Name).CurrentValue = 0;
                        }
                        entry.State = EntityState.Added;
                    }
                    else
                    {
                        entry.State = change.State;
                    }
                }
                await secondary.SaveChangesAsync(ct);
            }

            return result;
        }
    }
}
