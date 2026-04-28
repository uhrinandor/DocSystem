namespace DocSystem.TypeContracts;

public class IktatoszamReference : Entity
{
    public int Foszam { get; set; }
    public int Alszam { get; set; }
    public bool Valid { get; set; }
    public string SzovegesIktatoszam { get; set; } = string.Empty;
    public IktatokonyvReference Iktatokonyv { get; set; } = new();
}
