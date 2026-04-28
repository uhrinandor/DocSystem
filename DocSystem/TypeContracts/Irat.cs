namespace DocSystem.TypeContracts;

public class Irat : IratReference
{
    public IktatoszamReference Iktatoszam { get; set; } = new();
    public UgyiratReference Ugyirat { get; set; } = new();
}
