using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class WriteDbContext(DbContextOptions<WriteDbContext> options)
    : DbContext(options), IWriteDbContext
{
}
