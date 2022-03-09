#region Using

using MicroAutomation.Licensing.Data.Entities;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;

#endregion Using

namespace MicroAutomation.Licensing.Api.Service.Configurations;

public class SieveProcessorConfigurator : SieveProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SieveProcessorConfigurator"/> class.
    /// </summary>
    /// <param name="options"></param>
    public SieveProcessorConfigurator(IOptions<SieveOptions> options) : base(options)
    { }

    /// <summary>
    /// Configure the property mappers.
    /// </summary>
    /// <param name="mapper"></param>
    /// <returns></returns>
    protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
    {
        // Product mapper configurations
        mapper.Property<ProductEntity>(p => p.Id).CanFilter().CanSort();
        mapper.Property<ProductEntity>(p => p.Name).CanFilter().CanSort();
        mapper.Property<ProductEntity>(p => p.Description).CanFilter().CanSort();
        mapper.Property<ProductEntity>(p => p.Company).CanFilter().CanSort();
        mapper.Property<ProductEntity>(p => p.CreatedDataBy).CanFilter().CanSort();
        mapper.Property<ProductEntity>(p => p.CreatedDataUtc).CanFilter().CanSort();

        // License mapper configurations
        mapper.Property<LicenseEntity>(p => p.Id).CanFilter().CanSort();
        mapper.Property<LicenseEntity>(p => p.Name).CanFilter().CanSort();
        mapper.Property<LicenseEntity>(p => p.Email).CanFilter().CanSort();
        mapper.Property<LicenseEntity>(p => p.Company).CanFilter().CanSort();
        mapper.Property<LicenseEntity>(p => p.Type).CanFilter().CanSort();
        mapper.Property<LicenseEntity>(p => p.AdditionalAttributes).CanFilter();
        mapper.Property<LicenseEntity>(p => p.ProductFeatures).CanFilter();
        mapper.Property<LicenseEntity>(p => p.CreatedDataBy).CanFilter().CanSort();
        mapper.Property<LicenseEntity>(p => p.CreatedDataUtc).CanFilter().CanSort();

        return mapper;
    }
}