namespace DocSystem.Models;

public class FoszamCounter : Entity
{
    public int Next { get; set; }

    public Guid IktatokonyvId { get; set; }
    public Iktatokonyv Iktatokonyv { get; set; } = null!;
}