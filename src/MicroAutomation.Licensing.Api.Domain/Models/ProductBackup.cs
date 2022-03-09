#region Using

using AutoMapper;
using MicroAutomation.Licensing.Data.Entities;
using System;

#endregion Using

namespace MicroAutomation.Licensing.Api.Domain.Models;

/// <summary>
/// Product backup model.
/// </summary>
public class ProductBackup
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string PassPhrase { get; set; }

    public string PrivateKey { get; set; }

    public string PublicKey { get; set; }
}

/// <summary>
/// Product backup configuration for maps.
/// </summary>
public class ProductBackupProfile : Profile
{
    public ProductBackupProfile()
    {
        CreateMap<ProductEntity, ProductBackup>()
            .ForMember(dest => dest.PassPhrase, o => o.Ignore())
            .ForMember(dest => dest.PrivateKey, o => o.Ignore())
            .ForMember(dest => dest.PublicKey, o => o.Ignore())
            .ReverseMap();
    }
}