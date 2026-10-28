using System.ComponentModel;
using System.Globalization;
using System.Drawing;

namespace OceanicCosts;

public sealed class MainForm : Form
{
    private readonly CsvTarolo tarolo = new(Path.Combine(AppContext.BaseDirectory, "yacht_koltsegek_2024.csv"));
    private readonly BindingList<Koltseg> koltsegek = new();
    private readonly DataGridView tabla = new();
    private readonly TextBox nev = new();
    private readonly DateTimePicker datum = new();
    private readonly ComboBox kategoria = new();
    private readonly NumericUpDown osszeg = new();
    private readonly TextBox megjegyzes = new();
    private readonly Button hozzaadas = new();
    private readonly Label informacio = new();

    public MainForm()
    {
        Text = "Oceanic Dreams – Yacht karbantartási költségnyilvántartás";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(790, 540);
        Size = new Size(1050, 740);
        Font = new Font("Segoe UI", 10);
        BackColor = Color.FromArgb(246, 250, 248);

        var main = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), RowCount = 3, ColumnCount = 1 };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 205));
        Controls.Add(main);

        var heading = new Label { Text = "Yacht karbantartási költségek", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(25, 57, 51) };
        main.Controls.Add(heading, 0, 0);

        tabla.Dock = DockStyle.Fill;
        tabla.AutoGenerateColumns = false;
        tabla.AllowUserToAddRows = false;
        tabla.AllowUserToDeleteRows = false;
        tabla.ReadOnly = true;
        tabla.MultiSelect = false;
        tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabla.RowHeadersVisible = false;
        tabla.BackgroundColor = Color.White;
        tabla.DataSource = koltsegek;
        tabla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Koltseg.Id), HeaderText = "ID", FillWeight = 30 });
        tabla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Koltseg.YachtName), HeaderText = "Yacht neve", FillWeight = 125 });
        tabla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Koltseg.Datum), HeaderText = "Dátum", DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" }, FillWeight = 75 });
        tabla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Koltseg.Kategoria), HeaderText = "Kategória", FillWeight = 105 });
        tabla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Koltseg.Osszeg), HeaderText = "Összeg (€)", DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }, FillWeight = 75 });
        tabla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Koltseg.Megjegyzes), HeaderText = "Megjegyzés", FillWeight = 135 });
        main.Controls.Add(tabla, 0, 1);

        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8, 14, 8, 2), ColumnCount = 4, RowCount = 4 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        for (int i = 0; i < 4; i++) panel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        main.Controls.Add(panel, 0, 2);

        AddField(panel, "Yacht neve:", nev, 0, 0);
        nev.MaxLength = 100;
        AddField(panel, "Dátum:", datum, 2, 0);
        datum.Format = DateTimePickerFormat.Custom;
        datum.CustomFormat = "yyyy-MM-dd";
        AddField(panel, "Kategória:", kategoria, 0, 1);
        kategoria.DropDownStyle = ComboBoxStyle.DropDownList;
        kategoria.Items.AddRange(new object[] { "Maintenance", "Repairs", "Insurance", "Docking Fees", "Other" });
        kategoria.SelectedIndex = 0;
        AddField(panel, "Összeg (€):", osszeg, 2, 1);
        osszeg.Minimum = 0.01M;
        osszeg.Maximum = 1000000000M;
        osszeg.DecimalPlaces = 2;
        osszeg.ThousandsSeparator = true;
        osszeg.Value = 1000M;
        AddField(panel, "Megjegyzés:", megjegyzes, 0, 2);
        megjegyzes.MaxLength = 300;
        panel.SetColumnSpan(megjegyzes, 3);

        hozzaadas.Text = "Hozzáadás és mentés";
        hozzaadas.Dock = DockStyle.Fill;
        hozzaadas.BackColor = Color.FromArgb(114, 221, 120);
        hozzaadas.FlatStyle = FlatStyle.Flat;
        hozzaadas.Click += (_, _) => Hozzaadas();
        panel.Controls.Add(hozzaadas, 1, 3);
        informacio.Dock = DockStyle.Fill;
        informacio.TextAlign = ContentAlignment.MiddleLeft;
        panel.Controls.Add(informacio, 3, 3);

        try
        {
            foreach (var item in tarolo.Betoltes()) koltsegek.Add(item);
            informacio.Text = $"Betöltve: {koltsegek.Count} sor";
        }
        catch (Exception ex) when (ex is IOException or FormatException or OverflowException)
        {
            MessageBox.Show(this, ex.Message, "CSV betöltési hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            hozzaadas.Enabled = false;
        }
    }

    private static void AddField(TableLayoutPanel panel, string label, Control control, int x, int y)
    {
        panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, x, y);
        control.Dock = DockStyle.Fill;
        panel.Controls.Add(control, x + 1, y);
    }

    private void Hozzaadas()
    {
        string yachtNev = nev.Text.Trim();
        if (yachtNev.Length == 0)
        {
            MessageBox.Show(this, "Adja meg a yacht nevét!", "Hiányzó adat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            nev.Focus();
            return;
        }
        if (koltsegek.Any(k => k.Id == int.MaxValue))
        {
            MessageBox.Show(this, "Nem hozható létre új azonosító.");
            return;
        }
        int kovetkezoId = koltsegek.Count == 0 ? 1 : koltsegek.Max(k => k.Id) + 1;
        var uj = new Koltseg
        {
            Id = kovetkezoId,
            YachtName = yachtNev,
            Datum = datum.Value.Date,
            Kategoria = kategoria.SelectedItem?.ToString() ?? "Other",
            Osszeg = osszeg.Value,
            Megjegyzes = megjegyzes.Text.Trim()
        };

        // Először lementjük a bővített teljes adathalmazt; a felületet utána frissítjük.
        try
        {
            tarolo.Mentes(koltsegek.Concat(new[] { uj }));
            koltsegek.Add(uj);
            tabla.FirstDisplayedScrollingRowIndex = koltsegek.Count - 1;
            tabla.ClearSelection();
            tabla.Rows[koltsegek.Count - 1].Selected = true;
            informacio.Text = $"Mentve: #{uj.Id} • {koltsegek.Count} bejegyzés";
            nev.Clear();
            megjegyzes.Clear();
            osszeg.Value = 1000M;
            kategoria.SelectedIndex = 0;
            nev.Focus();
        }
        catch (IOException ex)
        {
            MessageBox.Show(this, $"A fájl mentése sikertelen: {ex.Message}", "Mentési hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
