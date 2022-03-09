#region Using

using MicroAutomation.Licensing.Data.Configuration.Configuration;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

#endregion Using

namespace MicroAutomation.Licensing.Data.Configuration.PostgreSQL;

public static class DatabaseExtensions
{
    /// <summary>
    /// Register DbContexts
    /// Configure the connection strings in AppSettings.json
    /// </summary>
    /// <typeparam name="TDataStoreDbContext"></typeparam>
    /// <typeparam name="TDataProtectionDbContext"></typeparam>
    /// <param name="services"></param>
    /// <param name="connectionStrings"></param>
    /// <param name="databaseMigrations"></param>
    public static void RegisterNpgSqlDbContexts<TDataStoreDbContext, TDataProtectionDbContext>(this IServiceCollection services,
        ConnectionStringsConfiguration connectionStrings,
        DatabaseMigrationsConfiguration databaseMigrations)
        where TDataStoreDbContext : DbContext
        where TDataProtectionDbContext : DbContext, IDataProtectionKeyContext
    {
        var migrationsAssembly = typeof(DatabaseExtensions).GetTypeInfo().Assembly.GetName().Name;

        // Config DB for DataStore
        services.AddDbContext<TDataStoreDbContext>(options => options.UseNpgsql(connectionStrings.DataStoreDbConnection,
            optionsSql => optionsSql.MigrationsAssembly(databaseMigrations.DataStoreDbMigrationsAssembly ?? migrationsAssembly)));

        // Config DB for DataProtection
        services.AddDbContext<TDataProtectionDbContext>(options => options.UseNpgsql(connectionStrings.DataProtectionDbConnection,
            optionsSql => optionsSql.MigrationsAssembly(databaseMigrations.DataProtectionDbMigrationsAssembly ?? migrationsAssembly)));
    }
}