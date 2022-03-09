namespace MicroAutomation.Licensing.Data.Configuration.Configuration;

public class DatabaseMigrationsConfiguration
{
    public bool ApplyDatabaseMigrations { get; set; } = false;

    public string DataStoreDbMigrationsAssembly { get; set; }
    public string AuditLogDbMigrationsAssembly { get; set; }
    public string DataProtectionDbMigrationsAssembly { get; set; }

    public void SetMigrationsAssemblies(string commonMigrationsAssembly)
    {
        DataStoreDbMigrationsAssembly = commonMigrationsAssembly;
        AuditLogDbMigrationsAssembly = commonMigrationsAssembly;
        DataProtectionDbMigrationsAssembly = commonMigrationsAssembly;
    }
}