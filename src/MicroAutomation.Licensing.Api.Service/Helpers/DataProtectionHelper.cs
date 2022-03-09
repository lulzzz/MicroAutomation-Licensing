using Azure.Identity;
using MicroAutomation.Licensing.Api.Service.Configurations;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MicroAutomation.Licensing.Api.Service.Helpers;

public static class DataProtectionHelper
{
    public static void AddDataProtection<TDataProtectionDbContext>(this IServiceCollection services, LicensingConfiguration configuration)
        where TDataProtectionDbContext : DbContext, IDataProtectionKeyContext
    {
        AddDataProtection<TDataProtectionDbContext>(
            services,
            configuration.DataProtection,
            configuration.AzureKeyVault);
    }

    private static void AddDataProtection<TDataProtectionDbContext>(this IServiceCollection services, DataProtectionConfiguration dataProtectionConfiguration, AzureKeyVaultConfiguration azureKeyVaultConfiguration)
        where TDataProtectionDbContext : DbContext, IDataProtectionKeyContext
    {
        var dataProtectionBuilder = services.AddDataProtection()
            .SetApplicationName("MicroAutomation.Licensing")
            .PersistKeysToDbContext<TDataProtectionDbContext>();

        if (dataProtectionConfiguration.ProtectKeysWithAzureKeyVault)
        {
            if (azureKeyVaultConfiguration.UseClientCredentials)
            {
                dataProtectionBuilder.ProtectKeysWithAzureKeyVault(
                    new Uri(azureKeyVaultConfiguration.DataProtectionKeyIdentifier),
                    new ClientSecretCredential(azureKeyVaultConfiguration.TenantId,
                        azureKeyVaultConfiguration.ClientId, azureKeyVaultConfiguration.ClientSecret));
            }
            else
            {
                dataProtectionBuilder.ProtectKeysWithAzureKeyVault(new Uri(azureKeyVaultConfiguration.DataProtectionKeyIdentifier), new DefaultAzureCredential());
            }
        }
    }
}