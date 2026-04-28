namespace DocSystem.Models;


public class Iktatoszam : Entity
{
    public int Foszam { get; set; }
    public int Alszam { get; set; }
    public bool Valid { get; set; } = false;
    public Guid IktatokonyvId { get; set; }
    public Iktatokonyv Iktatokonyv { get; set; } = null!;

    public AlszamCounter? AlszamCounter { get; set; }
}