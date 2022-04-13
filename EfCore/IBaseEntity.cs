using System.Text.Json.Serialization;

namespace projecthelper.EfCore;

/// <summary>
/// base for entity in cosmos.
/// </summary>
public interface IBaseEntity
{
    /// <summary>
    /// Id of object in cosmos
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// user that create object
    /// </summary>
    public string CreatedBy { get; }

    /// <summary>
    /// creation date
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// last updated user
    /// </summary>
    public string UpdatedBy { get; }

    /// <summary>
    /// last updated date
    /// </summary>
    public DateTime UpdatedAt { get; }

    /// <summary>
    /// item is virtual deleted
    /// </summary>
    public bool IsDeleted { get; }
}


public abstract record BaseEntity(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("isDeleted")]
    bool IsDeleted,
    [property: JsonPropertyName("createdBy")]
    string CreatedBy,
    [property: JsonPropertyName("createdAt")]
    DateTime CreatedAt,
    [property: JsonPropertyName("updatedBy")]
    string UpdatedBy,
    [property: JsonPropertyName("updatedAt")]
    DateTime UpdatedAt
) : IBaseEntity;

public abstract class BaseClassEntity : IBaseEntity
{
    public Guid Id { get; set; }

    public string CreatedBy { get; set; }

    public DateTime CreatedAt { set; get; }

    public string UpdatedBy { set; get; }

    public DateTime UpdatedAt { set; get; }

    public bool IsDeleted { set; get; }
}
