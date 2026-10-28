using System.Globalization;
using System.Text;

namespace OceanicCosts;

public sealed class CsvTarolo
{
    private const string Header = "id;yachtname;datum;kategoria;osszeg;megjegyzes";
    public string FajlUtvonal { get; }

    public CsvTarolo(string fajlUtvonal) => FajlUtvonal = fajlUtvonal;

    public List<Koltseg> Betoltes()
    {
        if (!File.Exists(FajlUtvonal))
            throw new FileNotFoundException("A yacht_koltsegek_2024.csv nem található.", FajlUtvonal);

        return File.ReadAllLines(FajlUtvonal, Encoding.UTF8)
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line =>
            {
                List<string> mezok = ParseCsvLine(line);
                if (mezok.Count != 6)
                    throw new FormatException($"Hibás CSV sor: {line}");
                return new Koltseg
                {
                    Id = int.Parse(mezok[0], CultureInfo.InvariantCulture),
                    YachtName = mezok[1],
                    Datum = DateTime.ParseExact(mezok[2], "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Kategoria = mezok[3],
                    Osszeg = decimal.Parse(mezok[4], CultureInfo.InvariantCulture),
                    Megjegyzes = mezok[5]
                };
            })
            .ToList();
    }

    public void Mentes(IEnumerable<Koltseg> lista)
    {
        var sorok = new List<string> { Header };
        foreach (Koltseg k in lista)
        {
            sorok.Add(string.Join(";", new[]
            {
                k.Id.ToString(CultureInfo.InvariantCulture),
                Escape(k.YachtName),
                k.Datum.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Escape(k.Kategoria),
                k.Osszeg.ToString(CultureInfo.InvariantCulture),
                Escape(k.Megjegyzes)
            }));
        }
        // Először ideiglenes fájlba írunk, hogy mentési hiba esetén megmaradjon az eredeti.
        string temp = FajlUtvonal + ".tmp";
        try
        {
            File.WriteAllLines(temp, sorok, new UTF8Encoding(false));
            File.Move(temp, FajlUtvonal, true);
        }
        finally
        {
            if (File.Exists(temp)) File.Delete(temp);
        }
    }

    private static string Escape(string value)
    {
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool insideQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else insideQuotes = !insideQuotes;
            }
            else if (c == ';' && !insideQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else current.Append(c);
        }
        if (insideQuotes) throw new FormatException("Lezáratlan idézőjel a CSV sorban.");
        result.Add(current.ToString());
        return result;
    }
}
