using System.Globalization;
using System.Text;
using OceanicStats;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo culture = CultureInfo.GetCultureInfo("hu-HU");
string path = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "yacht_berlesek_2024.csv");

try
{
    List<Berles> berlesek = File.ReadLines(path, Encoding.UTF8)
        .Skip(1) // Az első sor a fejléc.
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .Select(ParseBerles)
        .ToList();

    if (berlesek.Count == 0)
    {
        Console.WriteLine("A CSV nem tartalmaz bérléseket.");
        return;
    }

    int honap;
    while (true)
    {
        Console.Write("Adjon meg egy hónapot (1-12): ");
        if (int.TryParse(Console.ReadLine(), out honap) && honap is >= 1 and <= 12)
            break;
        Console.WriteLine("Hibás hónap. 1 és 12 közötti számot adjon meg!");
    }

    DateTime elsoNap = new(2024, honap, 1);
    DateTime utolsoNap = elsoNap.AddMonths(1).AddDays(-1);

    // A mintának megfelelően, ha akár egyetlen nap átfed, a TELJES bérleti díjat számoljuk.
    decimal haviBevetel = berlesek
        .Where(b => b.StartDate.Date <= utolsoNap && b.EndDate.Date >= elsoNap)
        .Sum(b => b.TotalPrice);

    decimal evesBevetel = berlesek.Sum(b => b.TotalPrice);
    Berles legdragabb = berlesek.MaxBy(b => b.TotalPrice)!;
    int yachtokSzama = berlesek.Select(b => b.YachtId).Distinct().Count();
    var leggyakoribb = berlesek.GroupBy(b => b.Name)
        .OrderByDescending(g => g.Count())
        .ThenBy(g => g.Key)
        .First();
    double atlagIdotartam = berlesek.Average(b => b.DurationDays);

    Console.WriteLine($"A(z) {honap}. hónap bevétele: {haviBevetel.ToString("N0", culture)} euró");
    Console.WriteLine($"A teljes 2024-es éves bevétel: {evesBevetel.ToString("N0", culture)} euró");
    Console.WriteLine($"A legdrágább bérlés az {legdragabb.Name} yacht volt, teljes ár: {legdragabb.TotalPrice.ToString("N0", culture)} euró");
    Console.WriteLine($"Összesen {yachtokSzama} különböző yachtot béreltek ki.");
    Console.WriteLine($"A legtöbbször bérelt yacht: {leggyakoribb.Key} ({leggyakoribb.Count()} bérlés)");
    Console.WriteLine($"Átlagos bérlési időtartam: {atlagIdotartam.ToString("N2", culture)} nap");
}
catch (Exception ex) when (ex is IOException or FormatException or OverflowException)
{
    Console.Error.WriteLine($"Nem sikerült feldolgozni a CSV-fájlt: {ex.Message}");
    Environment.ExitCode = 1;
}

static Berles ParseBerles(string line)
{
    string[] parts = line.Split(';');
    if (parts.Length != 6)
        throw new FormatException($"Hibás CSV-sor: {line}");

    return new Berles
    {
        Uid = int.Parse(parts[0], CultureInfo.InvariantCulture),
        YachtId = int.Parse(parts[1], CultureInfo.InvariantCulture),
        StartDate = DateTime.ParseExact(parts[2], "yyyy-MM-dd", CultureInfo.InvariantCulture),
        EndDate = DateTime.ParseExact(parts[3], "yyyy-MM-dd", CultureInfo.InvariantCulture),
        DailyPrice = decimal.Parse(parts[4], CultureInfo.InvariantCulture),
        Name = parts[5].Trim()
    };
}
