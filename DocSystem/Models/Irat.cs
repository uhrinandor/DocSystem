namespace DocSystem.Models;

public class Irat : Entity
{
    public Iktatoszam Iktatoszam { get; set; }
    
    public String Subject { get; set; }
    public String Details { get; set; }
    
    public Ugyirat Ugyirat { get; set; }
}