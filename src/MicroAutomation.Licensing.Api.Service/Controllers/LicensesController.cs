#region Using

using MicroAutomation.Licensing.Api.Domain.Extensions;
using MicroAutomation.Licensing.Api.Domain.Models;
using MicroAutomation.Licensing.Api.Domain.Services;
using MicroAutomation.Licensing.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#endregion Using

namespace MicroAutomation.Licensing.Api.Service.Controllers;

/// <summary>
/// Product licenses controller.
/// </summary>
[ApiController]
[Route("v1/products/{productId:guid}/licenses")]
public class LicensesController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly LicenseService _licenseService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LicensesController"/> class.
    /// </summary>
    /// <param name="productService"></param>
    /// <param name="licenseService"></param>
    public LicensesController(ProductService productService, LicenseService licenseService)
    {
        _productService = productService;
        _licenseService = licenseService;
    }

    /// <summary>
    /// Get all product licenses.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LicenseEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<LicenseEntity>>> ListAsync(Guid productId, [FromQuery] SieveModel request, CancellationToken cancellationToken)
        => Ok(await _licenseService.ListAsync(productId, request, cancellationToken));

    /// <summary>
    /// Get product license by identifier.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="licenseId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{licenseId:guid}")]
    [ProducesResponseType(typeof(ICollection<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetAsync(Guid productId, Guid licenseId, CancellationToken cancellationToken)
        => Ok(await _licenseService.GetAsync(productId, licenseId, cancellationToken));

    /// <summary>
    /// Add product licenses.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> AddAsync(Guid productId, LicenseRequest request, CancellationToken cancellationToken)
    {
        var license = await _licenseService.AddAsync(productId, request, cancellationToken);
        return CreatedAtAction(
            actionName: nameof(GetAsync),
            routeValues: new { productId = license.ProductId, licenseId = license.Id },
            value: license);
    }

    /// <summary>
    /// Import a product license.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="license"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("import")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ICollection<Product>>> ImportAsync(Guid productId, LicenseBackup license, CancellationToken cancellationToken)
        => Ok(await _licenseService.ImportAsync(productId, license, cancellationToken));

    /// <summary>
    /// Export a product license by identifier.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="licenseId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("{licenseId:guid}/export")]
    [ProducesResponseType(typeof(LicenseBackup), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LicenseBackup>> ExportAsync(Guid productId, Guid licenseId, CancellationToken cancellationToken)
        => Ok(await _licenseService.ExportAsync(productId, licenseId, cancellationToken));

    /// <summary>
    /// Generate a product license.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="licenseId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{licenseId:guid}/generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GenerateAsync(Guid productId, Guid licenseId, CancellationToken cancellationToken)
    {
        var product = await _productService.GetAsync(productId, cancellationToken);
        var license = await _licenseService.GenerateAsync(productId, licenseId, cancellationToken);
        using var memStream = new MemoryStream();
        license.Save(memStream);
        return File(memStream.ToArray(), "application/xml", $"{product.Name}-{licenseId}.xml".ToLower());
    }

    /// <summary>
    /// Delete a product license.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="licenseId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{licenseId:guid}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveAsync(Guid productId, Guid licenseId, CancellationToken cancellationToken)
    {
        await _licenseService.RemoveAsync(productId, licenseId, cancellationToken);
        return NoContent();
    }
}