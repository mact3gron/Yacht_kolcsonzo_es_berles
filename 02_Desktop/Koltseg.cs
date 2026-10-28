namespace OceanicCosts;

public sealed class Koltseg
{
    public int Id { get; set; }
    public string YachtName { get; set; } = "";
    public DateTime Datum { get; set; }
    public string Kategoria { get; set; } = "";
    public decimal Osszeg { get; set; }
    public string Megjegyzes { get; set; } = "";
}
