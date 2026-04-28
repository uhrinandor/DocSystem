namespace DocSystem.Models;

public class Iktatokonyv : Entity
{
    public string Nev { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;
    public int Evszam { get; set; } = DateTime.Now.Year;

    public List<Iktatoszam> Iktatoszamok { get; set; } = new();

    public FoszamCounter FoszamCounter { get; set; }
}