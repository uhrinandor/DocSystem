namespace DocSystem.TypeContracts;

public class Ugyirat : UgyiratReference
{
    public IktatoszamReference Iktatoszam { get; set; } = new();
    public List<IratReference> Irats { get; set; } = new();
}
