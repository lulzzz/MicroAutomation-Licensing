#region Using

using AutoMapper;
using MicroAutomation.Licensing.Api.Domain.Exceptions;
using MicroAutomation.Licensing.Api.Domain.Extensions;
using MicroAutomation.Licensing.Api.Domain.Models;
using MicroAutomation.Licensing.Data.Entities;
using MicroAutomation.Licensing.Data.Shared.Constants;
using MicroAutomation.Licensing.Data.Shared.DbContexts;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sieve.Models;
using Sieve.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#endregion Using

namespace MicroAutomation.Licensing.Api.Domain.Services;

/// <summary>
/// A license service.
/// </summary>
public class LicenseService
{
    private readonly DataStoreDbContext _dataStore;
    private readonly ProductService _productService;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly IDataProtectionProvider _dataProtection;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseService"/> class.
    /// </summary>
    /// <param name="dataStore"></param>
    /// <param name="productService"></param>
    /// <param name="mapper"></param>
    /// <param name="sieveProcessor"></param>
    /// <param name="dataProtection"></param>
    /// <param name="logger"></param>
    public LicenseService(
        DataStoreDbContext dataStore,
        ProductService productService,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        IDataProtectionProvider dataProtection,
        ILogger<ProductService> logger)
    {
        _dataStore = dataStore;
        _productService = productService;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _dataProtection = dataProtection;
        _logger = logger;
    }

    /// <summary>
    /// Retrieve the product licenses from the data store using the pagination functionality.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<PagedResult<LicenseEntity>> ListAsync(Guid productId, SieveModel request, CancellationToken cancellationToken)
    {
        var productEntity = await _dataStore.Set<ProductEntity>()
            .AsNoTracking().Where(x => x.Id == productId).AnyAsync(cancellationToken);
        if (productEntity == default)
        {
            _logger.LogError("Unable to find the following product: {0}", productId);
            throw new NotFoundException($"Unable to find the following product: {productId}");
        }

        var query = _dataStore.Set<LicenseEntity>()
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .AsQueryable();
        return await _sieveProcessor.GetPagedAsync(query, request, cancellationToken);
    }

    /// <summary>
    /// Get product license by identifier.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="licenseId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<LicenseEntity> GetAsync(Guid productId, Guid licenseId, CancellationToken cancellationToken)
    {
        var productEntity = await _dataStore.Set<ProductEntity>()
            .AsNoTracking().Where(x => x.Id == productId).AnyAsync(cancellationToken);
        if (productEntity == default)
        {
            _logger.LogError("Unable to find the following product: {0}", productId);
            throw new NotFoundException($"Unable to find the following product: {productId}");
        }

        var licenseEntity = await _dataStore.Set<LicenseEntity>()
            .Where(x => x.Id == licenseId)
            .FirstOrDefaultAsync(cancellationToken);
        if (licenseEntity == default)
        {
            _logger.LogError("Unable to find the following product license: {0}", licenseId);
            throw new NotFoundException($"Unable to find the following product license: {licenseId}");
        }

        return licenseEntity;
    }

    /// <summary>
    /// Delete product license by identifier.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="licenseId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task RemoveAsync(Guid productId, Guid licenseId, CancellationToken cancellationToken)
    {
        var productEntity = await _dataStore.Set<ProductEntity>()
       .AsNoTracking().Where(x => x.Id == productId).AnyAsync(cancellationToken);
        if (productEntity == default)
        {
            _logger.LogError("Unable to find the following product: {0}", productId);
            throw new NotFoundException($"Unable to find the following product: {productId}");
        }

        var licenseEntity = await GetAsync(productId, licenseId, cancellationToken);
        _dataStore.Remove(licenseEntity);
        await _dataStore.SaveChangesAsync(cancellationToken);
    }

    public async Task<LicenseEntity> AddAsync(Guid productId, LicenseRequest request, CancellationToken cancellationToken)
    {
        var productEntity = await _dataStore.Set<ProductEntity>()
            .AsNoTracking().Where(x => x.Id == productId).AnyAsync(cancellationToken);
        if (productEntity == default)
        {
            _logger.LogError("Unable to find the following product: {0}", productId);
            throw new NotFoundException($"Unable to find the following product: {productId}");
        }

        var license = _mapper.Map<LicenseEntity>(request);
        license.ProductId = productId;
        await _dataStore.Set<LicenseEntity>().AddAsync(license, cancellationToken);
        await _dataStore.SaveChangesAsync(cancellationToken);
        return license;
    }

    public async Task<License> GenerateAsync(Guid productId, Guid licenseId, CancellationToken cancellationToken)
    {
        // Retrieve the elements
        var product = await _productService.GetAsync(productId, cancellationToken);
        var licenseEntity = await GetAsync(productId, licenseId, cancellationToken);

        // Unprotect private key of the product.
        var protector = _dataProtection.CreateProtector(DataProtectionConsts.DefaultPurpose);
        var passPhrase = protector.Unprotect(product.PassPhrase);
        var privateKey = protector.Unprotect(product.PrivateKey);

        // Generate license data
        var licenseBuild = License.New();
        licenseBuild.WithUniqueIdentifier(licenseEntity.Id);
        licenseBuild.WithProduct(product.Id, product.Name);
        licenseBuild.As((LicenseType)Enum.Parse(typeof(LicenseType), licenseEntity.Type, true));
        licenseBuild.ExpiresAt(licenseEntity.ExpiresAt);
        if (licenseEntity.AdditionalAttributes?.Count > 0)
            licenseBuild.WithAdditionalAttributes(licenseEntity.AdditionalAttributes);
        if (licenseEntity.ProductFeatures?.Count > 0)
            licenseBuild.WithProductFeatures(licenseEntity.ProductFeatures);
        licenseBuild.LicensedTo(x =>
        {
            x.Email = licenseEntity.Email;
            x.Company = product.Company;
            x.Name = licenseEntity.Name;
        });

        return licenseBuild.CreateAndSignWithPrivateKey(privateKey, passPhrase);
    }

    public async Task<LicenseBackup> ExportAsync(Guid productId, Guid licenseId, CancellationToken cancellationToken)
    {
        var licenseEntity = await GetAsync(productId, licenseId, cancellationToken);
        return _mapper.Map<LicenseBackup>(licenseEntity);
    }

    public async Task<LicenseEntity> ImportAsync(Guid productId, LicenseBackup backup, CancellationToken cancellationToken)
    {
        var licence = _mapper.Map<LicenseEntity>(backup);
        licence.ProductId = productId;
        await _dataStore.AddAsync(licence, cancellationToken);
        await _dataStore.SaveChangesAsync(cancellationToken);
        return licence;
    }
}