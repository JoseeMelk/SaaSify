namespace SaaSify.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    // Cualquier entidad puede marcarse como modificada
    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    // Eliminación lógica — el registro sigue en la DB
    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
    }

    public bool IsDeleted => DeletedAt.HasValue;
}