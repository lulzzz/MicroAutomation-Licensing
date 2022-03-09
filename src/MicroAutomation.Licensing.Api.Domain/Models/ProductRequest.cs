#region Using

using AutoMapper;
using FluentValidation;
using MicroAutomation.Licensing.Data.Entities;

#endregion Using

namespace MicroAutomation.Licensing.Api.Domain.Models;

/// <summary>
/// Product request model.
/// </summary>
public class ProductRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Company { get; set; }
}

/// <summary>
/// Product request configuration for maps.
/// </summary>
public class ProductRequestProfile : Profile
{
    public ProductRequestProfile()
    {
        CreateMap<ProductRequest, ProductEntity>();
    }
}

/// <summary>
/// Product request validator.
/// </summary>
public class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Name).Length(4, 64);

        RuleFor(x => x.Description).Length(0, 128);

        RuleFor(x => x.Company).Length(4, 64);
    }
}