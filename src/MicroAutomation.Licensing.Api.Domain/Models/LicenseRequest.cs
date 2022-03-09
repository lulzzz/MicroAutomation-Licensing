#region Using

using AutoMapper;
using FluentValidation;
using MicroAutomation.Licensing.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;

#endregion Using

namespace MicroAutomation.Licensing.Api.Domain.Models;

/// <summary>
/// License request model.
/// </summary>
public class LicenseRequest
{
    [DefaultValue("")]
    public string Name { get; set; }

    [DefaultValue("")]
    public string Email { get; set; }

    [DefaultValue(null)]
    public string Company { get; set; }

    public LicenseType Type { get; set; }

    [DefaultValue(7)]
    public int Duration { get; set; }

    [DefaultValue(null)]
    public Dictionary<string, string> AdditionalAttributes { get; set; }

    [DefaultValue(null)]
    public Dictionary<string, string> ProductFeatures { get; set; }
}

/// <summary>
/// License request configuration for maps.
/// </summary>
public class LicenseRequestProfile : Profile
{
    public LicenseRequestProfile()
    {
        CreateMap<LicenseRequest, LicenseEntity>()
             .ForMember(dest => dest.ExpiresAt, o => o.MapFrom(src => DateTime.Now.AddDays(src.Duration)));
    }
}

/// <summary>
/// License request validator.
/// </summary>
public class LicenseRequestValidator : AbstractValidator<LicenseRequest>
{
    public LicenseRequestValidator()
    {
        RuleFor(x => x.Name).NotNull();
        RuleFor(x => x.Name).Length(4, 64);

        RuleFor(x => x.Email).NotNull();
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Email).Length(4, 64);

        RuleFor(x => x.Company).Length(4, 64);

        RuleFor(x => x.Duration).NotNull();
        RuleFor(x => x.Duration).InclusiveBetween(1, 1460);
    }
}