namespace DocSystem.Models;

public class Ugyirat : Entity
{
    public List<Irat> Irats { get; set; }
    
    public Iktatoszam Iktatoszam { get; set; }
}