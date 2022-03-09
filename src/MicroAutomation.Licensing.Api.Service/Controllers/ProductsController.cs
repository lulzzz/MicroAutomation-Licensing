#region Using

using MicroAutomation.Licensing.Api.Domain.Extensions;
using MicroAutomation.Licensing.Api.Domain.Models;
using MicroAutomation.Licensing.Api.Domain.Services;
using MicroAutomation.Licensing.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

#endregion Using

namespace MicroAutomation.Licensing.Api.Service.Controllers;

/// <summary>
/// Product Controller.
/// </summary>
[ApiController]
[Route("v1/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsController"/> class.
    /// </summary>
    /// <param name="productService"></param>
    public ProductsController(ProductService productService) => _productService = productService;

    /// <summary>
    /// Get all products.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<ProductEntity>>> ListAsync([FromQuery] SieveModel request, CancellationToken cancellationToken)
        => Ok(await _productService.ListAsync(request, cancellationToken));

    /// <summary>
    /// Add a new product.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProductEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductEntity>> AddAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productService.AddAsync(request, cancellationToken);
        return CreatedAtAction(
            actionName: nameof(GetAsync),
            routeValues: new { productId = product.Id },
            value: product);
    }

    /// <summary>
    /// Import a product with form data.
    /// </summary>
    /// <param name="product"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("import")]
    [ProducesResponseType(typeof(ProductEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductEntity>> ImportAsync(ProductBackup product, CancellationToken cancellationToken)
        => Ok(await _productService.ImportAsync(product, cancellationToken));

    /// <summary>
    /// Get product by identifier.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{productId:guid}")]
    [ProducesResponseType(typeof(ProductEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductEntity>> GetAsync(Guid productId, CancellationToken cancellationToken)
        => Ok(await _productService.GetAsync(productId, cancellationToken));

    /// <summary>
    /// Update product by identifier.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{productId:guid}")]
    [ProducesResponseType(typeof(ProductEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductEntity>> UpdateAsync(Guid productId, ProductRequest request, CancellationToken cancellationToken)
        => Ok(await _productService.UpdateAsync(productId, request, cancellationToken));

    /// <summary>
    /// Export a product by identifier.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("{productId:guid}/export")]
    [ProducesResponseType(typeof(ProductBackup), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductBackup>> ExportAsync(Guid productId, CancellationToken cancellationToken)
        => Ok(await _productService.ExportAsync(productId, cancellationToken));

    /// <summary>
    /// Delete a product by identifier.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAsync(Guid productId, CancellationToken cancellationToken)
    {
        await _productService.RemoveAsync(productId, cancellationToken);
        return NoContent();
    }
}