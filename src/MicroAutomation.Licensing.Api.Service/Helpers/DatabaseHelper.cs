#region Using

using MicroAutomation.Licensing.Api.Service.Configurations;
using MicroAutomation.Licensing.Data.Configuration.Configuration;
using MicroAutomation.Licensing.Data.Configuration.PostgreSQL;
using MicroAutomation.Licensing.Data.Configuration.SqlServer;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using PostgreSQLMigrationAssembly = MicroAutomation.Licensing.Data.PostgreSQL.Helpers.MigrationAssembly;
using SqlMigrationAssembly = MicroAutomation.Licensing.Data.SQLServer.Helpers.MigrationAssembly;

#endregion Using

namespace MicroAutomation.Licensing.Api.Service.Helpers;

/// <summary>
/// Database helper.
/// </summary>
public static class DatabaseHelper
{
    /// <summary>
    /// Register DbContexts for this application.
    /// Configure the connection strings in AppSettings.json
    /// </summary>
    /// <typeparam name="TDataStoreDbContext"></typeparam>
    /// <typeparam name="TDataProtectionDbContext"></typeparam>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void RegisterDbContexts<TDataStoreDbContext, TDataProtectionDbContext>(this IServiceCollection services, LicensingConfiguration configuration)
        where TDataStoreDbContext : DbContext
        where TDataProtectionDbContext : DbContext, IDataProtectionKeyContext
    {
        if (configuration.Testing.IsStaging)
            services.RegisterDbContextsStaging<TDataStoreDbContext, TDataProtectionDbContext>();
        else
            services.RegisterDbContextsProduction<TDataStoreDbContext, TDataProtectionDbContext>(configuration);
    }

    private static void RegisterDbContextsStaging<TDataStoreDbContext, TDataProtectionDbContext>(this IServiceCollection services)
        where TDataStoreDbContext : DbContext
        where TDataProtectionDbContext : DbContext, IDataProtectionKeyContext
    {
        var dataStoreDatabaseName = Guid.NewGuid().ToString();
        var dataProtectionDatabaseName = Guid.NewGuid().ToString();

        services.AddDbContext<TDataStoreDbContext>(optionsBuilder => optionsBuilder.UseInMemoryDatabase(dataStoreDatabaseName));
        services.AddDbContext<TDataProtectionDbContext>(optionsBuilder => optionsBuilder.UseInMemoryDatabase(dataProtectionDatabaseName));
    }

    private static void RegisterDbContextsProduction<TDataStoreDbContext, TDataProtectionDbContext>(this IServiceCollection services, LicensingConfiguration configuration)
        where TDataStoreDbContext : DbContext
        where TDataProtectionDbContext : DbContext, IDataProtectionKeyContext
    {
        // Set Migration assembly
        var migrationsAssembly = GetMigrationAssemblyByProvider(configuration.DatabaseProvider);
        configuration.DatabaseMigrations.SetMigrationsAssemblies(migrationsAssembly);

        // Register the database provider.
        switch (configuration.DatabaseProvider.ProviderType)
        {
            case DatabaseProviderType.SqlServer:
                services.RegisterSqlServerDbContexts<TDataStoreDbContext, TDataProtectionDbContext>(configuration.ConnectionStrings, configuration.DatabaseMigrations);
                break;

            case DatabaseProviderType.PostgreSQL:
                services.RegisterNpgSqlDbContexts<TDataStoreDbContext, TDataProtectionDbContext>(configuration.ConnectionStrings, configuration.DatabaseMigrations);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(configuration.DatabaseProvider.ProviderType), $@"The value needs to be one of {string.Join(", ", Enum.GetNames(typeof(DatabaseProviderType)))}.");
        }
    }

    private static string GetMigrationAssemblyByProvider(DatabaseProviderConfiguration databaseProvider)
    {
        return databaseProvider.ProviderType switch
        {
            DatabaseProviderType.SqlServer => typeof(SqlMigrationAssembly).GetTypeInfo().Assembly.GetName().Name,
            DatabaseProviderType.PostgreSQL => typeof(PostgreSQLMigrationAssembly).GetTypeInfo().Assembly.GetName().Name,
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider.ProviderType), $@"The value needs to be one of {string.Join(", ", Enum.GetNames(typeof(DatabaseProviderType)))}.")
        };
    }
}