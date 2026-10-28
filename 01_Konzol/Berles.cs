namespace OceanicStats;

public sealed class Berles
{
    public int Uid { get; init; }
    public int YachtId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal DailyPrice { get; init; }
    public string Name { get; init; } = string.Empty;

    // A kezdő- és a zárónap is beleszámít az időtartamba.
    public int DurationDays => (EndDate.Date - StartDate.Date).Days + 1;
    public decimal TotalPrice => DurationDays * DailyPrice;
}
