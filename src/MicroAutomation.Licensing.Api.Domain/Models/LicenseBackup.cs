#region Using

using AutoMapper;
using MicroAutomation.Licensing.Data.Entities;
using System;
using System.Collections.Generic;

#endregion Using

namespace MicroAutomation.Licensing.Api.Domain.Models;

/// <summary>
/// License backup model.
/// </summary>
public class LicenseBackup
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Company { get; set; }
    public string Type { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, string> AdditionalAttributes { get; set; }

    public Dictionary<string, string> ProductFeatures { get; set; }
}

/// <summary>
/// License backup configuration for maps.
/// </summary>
public class LicenseBackupProfile : Profile
{
    public LicenseBackupProfile()
    {
        CreateMap<LicenseEntity, LicenseBackup>().ReverseMap();
    }
}