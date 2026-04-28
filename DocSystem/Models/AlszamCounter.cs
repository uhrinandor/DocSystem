namespace DocSystem.Models;

public class AlszamCounter : Entity
{
    public int Next { get; set; }

    public Guid IktatoszamId { get; set; }
    public Iktatoszam Iktatoszam { get; set; } = null!;
}