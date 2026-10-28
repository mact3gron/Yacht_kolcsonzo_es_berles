using System.ComponentModel.DataAnnotations;

namespace OceanicApi.Models;

public sealed class BerlesLetrehozas
{
    [Range(1, int.MaxValue, ErrorMessage = "A uid legyen pozitív egész szám.")]
    public int Uid { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A yachtId legyen pozitív egész szám.")]
    public int YachtId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A napi díjnak pozitívnak kell lennie.")]
    public int DailyPrice { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "A kaució nem lehet negatív.")]
    public int Deposit { get; set; }
}
