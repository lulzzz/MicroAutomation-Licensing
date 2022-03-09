#region Using

using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

#endregion Using

namespace MicroAutomation.Licensing.Data.Shared.DbContexts;

public class DataProtectionDbContext : DbContext, IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    public DataProtectionDbContext(DbContextOptions<DataProtectionDbContext> options)
        : base(options) { }
}