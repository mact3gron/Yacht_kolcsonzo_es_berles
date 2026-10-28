using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceanicApi.Models;

public sealed class Berles
{
    [Key]
    public int Id { get; set; }
    public int Uid { get; set; }
    public int YachtId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DailyPrice { get; set; }
    public int Deposit { get; set; }

    // Az adatbázis nem tárolja; a kezdő- és zárónap is fizetős.
    [NotMapped]
    public int TotalPrice => checked(((EndDate.Date - StartDate.Date).Days + 1) * DailyPrice);
}
