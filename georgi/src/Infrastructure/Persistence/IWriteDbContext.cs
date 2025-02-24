using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public interface IWriteDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
}
