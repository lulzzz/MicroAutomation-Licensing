#region Using

using AutoMapper;
using MicroAutomation.Licensing.Api.Domain.Exceptions;
using MicroAutomation.Licensing.Api.Domain.Extensions;
using MicroAutomation.Licensing.Api.Domain.Models;
using MicroAutomation.Licensing.Data.Entities;
using MicroAutomation.Licensing.Data.Shared.Constants;
using MicroAutomation.Licensing.Data.Shared.DbContexts;
using MicroAutomation.Licensing.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Security;
using Sieve.Models;
using Sieve.Services;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

#endregion Using

namespace MicroAutomation.Licensing.Api.Domain.Services;

/// <summary>
/// A product service.
/// </summary>
public class ProductService
{
    private readonly DataStoreDbContext _dataStore;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly IDataProtectionProvider _dataProtection;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductService"/> class.
    /// </summary>
    /// <param name="dataStore"></param>
    /// <param name="mapper"></param>
    /// <param name="logger"></param>
    public ProductService(DataStoreDbContext dataStore,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        IDataProtectionProvider dataProtection,
        ILogger<ProductService> logger)
    {
        _dataStore = dataStore;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _dataProtection = dataProtection;
        _logger = logger;
    }

    /// <summary>
    /// Retrieve the products from the data store using the pagination functionality.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<PagedResult<ProductEntity>> ListAsync(SieveModel request, CancellationToken cancellationToken)
    {
        var query = _dataStore.Set<ProductEntity>().AsNoTracking().AsQueryable();
        return await _sieveProcessor.GetPagedAsync(query, request, cancellationToken);
    }

    /// <summary>
    /// Create a new product.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ConflitException"></exception>
    /// <returns></returns>
    public async Task<ProductEntity> AddAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        // Detect if the request conflicts with the data store
        var detectConflit = await _dataStore.Set<ProductEntity>().AnyAsync(x => x.Name == request.Name, cancellationToken);
        if (detectConflit)
            throw new ConflitException($"The following product name already exists: {request.Name}");

        // Map the product request to product
        var product = _mapper.Map<ProductEntity>(request);

        // Create a new public/private key pair for your product
        var passPhrase = GeneratePassPhrase();
        var keyGenerator = KeyGenerator.Create();
        var keyPair = keyGenerator.GenerateKeyPair();
        var privateKey = keyPair.ToEncryptedPrivateKeyString(passPhrase);
        var publicKey = keyPair.ToPublicKeyString();

        // Protect sensitive data
        var protector = _dataProtection.CreateProtector(DataProtectionConsts.DefaultPurpose);
        product.PassPhrase = protector.Protect(passPhrase);
        product.PrivateKey = protector.Protect(privateKey);
        product.PublicKey = protector.Protect(publicKey);

        // Save data to the data store
        await _dataStore.Set<ProductEntity>().AddAsync(product, cancellationToken);
        await _dataStore.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("The following product has been created: {0}", product.Name);

        return product;
    }

    public Task<object> UpdateAsync(Guid productId, ProductRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieve the product from the data store.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="NotFoundException"></exception>
    /// <returns></returns>
    public async Task<ProductEntity> GetAsync(Guid productId, CancellationToken cancellationToken)
    {
        // Retrieve the product from the data store
        var productEntity = await _dataStore.Set<ProductEntity>()
            .Where(x => x.Id == productId)
            .FirstOrDefaultAsync(cancellationToken);

        // Return the product if it exists
        return productEntity == default ?
            throw new NotFoundException(nameof(ProductEntity))
            : productEntity;
    }

    /// <summary>
    /// Delete the product from the data store.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="NotFoundException"></exception>
    /// <returns></returns>
    public async Task RemoveAsync(Guid productId, CancellationToken cancellationToken)
    {
        // Retrieve the product from the datastore
        var productEntity = await GetAsync(productId, cancellationToken);

        // Delete product in data store
        _dataStore.Set<ProductEntity>().Remove(productEntity);
        await _dataStore.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("The following product has been deleted: {0}", productEntity.Name);
    }

    /// <summary>
    /// Generate new pass phrase to create a new public/private key pair.
    /// </summary>
    private static string GeneratePassPhrase()
    {
        using SHA256 sha256Hash = SHA256.Create();
        var generator = GeneratorUtilities.GetKeyGenerator("AES256");
        var key = generator.GenerateKey();
        return string.Concat(sha256Hash.ComputeHash(key).Select(x => x.ToString("X2")));
    }

    /// <summary>
    /// Export product configuration.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="NotFoundException"></exception>
    /// <returns></returns>
    public async Task<ProductBackup> ExportAsync(Guid productId, CancellationToken cancellationToken)
    {
        // Retrieve the product from the datastore
        var productEntity = await GetAsync(productId, cancellationToken);

        // Convert to backup object
        var productBackup = _mapper.Map<ProductBackup>(productEntity);

        // Unprotect the public/private key pair
        var protector = _dataProtection.CreateProtector(DataProtectionConsts.DefaultPurpose);
        productBackup.PassPhrase = protector.Unprotect(productEntity.PassPhrase);
        productBackup.PrivateKey = protector.Unprotect(productEntity.PrivateKey);
        productBackup.PublicKey = protector.Unprotect(productEntity.PublicKey);

        return productBackup;
    }

    /// <summary>
    /// Import product configuration.
    /// </summary>
    /// <param name="backup"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ConflitException"></exception>
    /// <exception cref="BadRequestExecption"></exception>
    public async Task<ProductEntity> ImportAsync(ProductBackup backup, CancellationToken cancellationToken)
    {
        // Detect if the request conflicts with the data store
        var detectNameConflit = await _dataStore.Set<ProductEntity>().AnyAsync(x => x.Name == backup.Name, cancellationToken);
        if (detectNameConflit)
            throw new ConflitException($"The following product name already exists: {backup.Name}");
        var detectIdConflit = await _dataStore.Set<ProductEntity>().AnyAsync(x => x.Id == backup.Id, cancellationToken);
        if (detectIdConflit)
            throw new ConflitException($"The following product identifier already exists: {backup.Id}");

        // Map the product request to product
        var product = _mapper.Map<ProductEntity>(backup);

        // Protect sensitive data
        var protector = _dataProtection.CreateProtector(DataProtectionConsts.DefaultPurpose);
        product.PassPhrase = protector.Protect(backup.PassPhrase);
        product.PrivateKey = protector.Protect(backup.PrivateKey);
        product.PublicKey = protector.Protect(backup.PublicKey);

        // Validate the public/private key pair
        if (!ValidateKeyPair(backup.PrivateKey, backup.PassPhrase, backup.PublicKey))
            throw new BadRequestExecption("Invalid public/private key pair");

        // Save data to the data store
        await _dataStore.Set<ProductEntity>().AddAsync(product, cancellationToken);
        await _dataStore.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("The following product has been imported: {0}", product.Name);

        return product;
    }

    /// <summary>
    /// Valid the public/private key pair.
    /// </summary>
    /// <param name="privateKey"></param>
    /// <param name="passPhrase"></param>
    /// <param name="publicKey"></param>
    /// <returns></returns>
    private bool ValidateKeyPair(string privateKey, string passPhrase, string publicKey)
    {
        try
        {
            var license = License.New().CreateAndSignWithPrivateKey(privateKey, passPhrase);
            return license.VerifySignature(publicKey);
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}