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
                foreach (var change in changes)
                {
                    var entry = secondary.Entry(change.Entity);
                    entry.State = change.State;
                    if (change.State == EntityState.Added)
                    {
                        var idProp = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault();
                        if (idProp != null)
                            entry.Property(idProp.Name).IsTemporary = false;
                    }
                }
                await secondary.SaveChangesAsync(ct);
            }

            return result;
        }
    }
}
