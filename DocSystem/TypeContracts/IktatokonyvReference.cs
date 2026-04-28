namespace DocSystem.TypeContracts;

public class IktatokonyvReference : Entity
{
    public string Nev { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;
    public int Evszam { get; set; }
}
