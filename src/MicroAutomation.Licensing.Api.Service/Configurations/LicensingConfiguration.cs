using MicroAutomation.Licensing.Data.Configuration.Configuration;
using Microsoft.Extensions.Configuration;

namespace MicroAutomation.Licensing.Api.Service.Configurations;

public class LicensingConfiguration
{
    /// <summary>
    /// The settings for test deployments.
    /// </summary>
    public TestingConfiguration Testing { get; set; } = new TestingConfiguration();

    /// <summary>
    /// The database connection strings and settings.
    /// </summary>
    public ConnectionStringsConfiguration ConnectionStrings { get; set; } = new ConnectionStringsConfiguration();

    /// <summary>
    /// The settings for the database provider.
    /// </summary>
    public DatabaseProviderConfiguration DatabaseProvider { get; set; } = new DatabaseProviderConfiguration();

    /// <summary>
    /// The settings for database migrations.
    /// </summary>
    public DatabaseMigrationsConfiguration DatabaseMigrations { get; set; } = new DatabaseMigrationsConfiguration();

    /// <summary>
    /// The settings for data protection.
    /// </summary>
    public DataProtectionConfiguration DataProtection { get; set; } = new DataProtectionConfiguration();

    /// <summary>
    /// The settings for Azure key vault.
    /// </summary>
    public AzureKeyVaultConfiguration AzureKeyVault { get; set; } = new AzureKeyVaultConfiguration();

    /// <summary>
    /// Applies configuration parsed from an appsettings file into these options.
    /// </summary>
    /// <param name="configuration">The configuration to bind into this instance.</param>
    public LicensingConfiguration(IConfiguration configuration)
    {
        configuration.GetSection(nameof(TestingConfiguration)).Bind(Testing);
        configuration.GetSection("ConnectionStrings").Bind(ConnectionStrings);
        configuration.GetSection(nameof(DatabaseProviderConfiguration)).Bind(DatabaseProvider);
        configuration.GetSection(nameof(DatabaseMigrationsConfiguration)).Bind(DatabaseMigrations);
        configuration.GetSection(nameof(DataProtectionConfiguration)).Bind(DataProtection);
        configuration.GetSection(nameof(AzureKeyVaultConfiguration)).Bind(AzureKeyVault);
    }
}