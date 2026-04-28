namespace DocSystem.TypeContracts;

public class Iktatokonyv : IktatokonyvReference
{
    public List<IktatoszamReference> Iktatoszamok { get; set; } = new();
}
