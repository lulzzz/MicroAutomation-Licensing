#region Using

using System;

#endregion Using

namespace MicroAutomation.Licensing.Data.Abstracts;

/// <summary>
/// This interface is implemented by entities that is wanted
/// to store creation information (who and when created).
/// Creation time and creator user are automatically set when saving Entity to database.
/// </summary>
public interface ICreationAuditable : ICreationTrackable
{
    Guid CreatedDataBy { get; set; }
}