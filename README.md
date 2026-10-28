# Oceanic Dreams – Yachtos beadandó

**Külkergimi · Szoftverfejlesztő és -tesztelő · 2026. október**


## Előkészületek

- **C# konzol:** .NET 8 SDK (Visual Studio 2022/2026 vagy VS Code + C# bővítmény).
- **Windows Forms desktop:** Windows + .NET 8 SDK / Visual Studio `Desktop development with .NET` munkaterhelés.
- **ASP.NET Core API:** .NET 8 SDK + internet a NuGet `Microsoft.EntityFrameworkCore.Sqlite` csomag első visszaállításához.
- **Weboldal:** bármely modern böngésző.
- **Frontend:** Node.js 20.11+ (külső npm-csomagok telepítése nélkül)

## 01 – Konzolos statisztika (15 pont)

PowerShellben a csomag gyökeréből:

```powershell
cd .\01_Konzol
dotnet run
```

`3`. A várható eredmények:

```text
A(z) 3. hónap bevétele: 10 334 388 euró
A teljes 2024-es éves bevétel: 71 681 844 euró
A legdrágább bérlés az O'PTASIA yacht volt, teljes ár: 621 530 euró
Összesen 18 különböző yachtot béreltek ki.
A legtöbbször bérelt yacht: CARPE DIEM (25 bérlés)
Átlagos bérlési időtartam: 7,82 nap
```

A **havi** összeg számításakor a teljes bérleti díj szerepel, ha a bérlés akár egy napra is átnyúlik az adott hónapba. A kezdő- és zárónap is beleszámít az időtartamba. A CSV automatikusan másolódik a futtatási könyvtárba.

## 02 – Desktop költségnyilvántartó (10 pont)

```powershell
cd .\02_Desktop
dotnet run
```

Windows Forms ablak nyílik: 300 eredeti bejegyzés táblázatban, új költség felvétele yacht névvel, dátummal, legördülő kategóriával, összeggel és opcionális megjegyzéssel. A **Hozzáadás és mentés** gomb az egész CSV-t újraírja, automatikusan új ID-t ad, majd a táblázatban azonnal megjeleníti a sort.

**Fontos:** a program a futási helyre bemásolt `yacht_koltsegek_2024.csv` fájlt frissíti (jellemzően `02_Desktop\bin\Debug\net8.0-windows\yacht_koltsegek_2024.csv`). Az eredeti `forrasanyag` fájl érintetlen marad. Az új adatok megtartásához a futtatási mappában lévő CSV-t mentsd el.

## 03 – Reszponzív bemutatkozó weboldal (10 pont)

Nyisd meg a `03_Weboldal/index.html` fájlt böngészőben. A `forras.txt` **teljes és pontos angol szövege**, a **3 megadott kép** és az előírt `rgb(114, 221, 120)` zöld szerepel. A navbar 992 px alatt hamburger menüre vált, a képek és szöveg 768 px alatt egymás alá kerülnek. A navbar keresője a feladat szerint itt még csak megjelenítendő elem; az éles kereső az 05-ös részben működik.

## 04 – Web API, ASP.NET Core + SQLite (15 pont)

```powershell
cd .\04_WebAPI
dotnet restore
dotnet run --urls http://localhost:5084
```

Első indításkor automatikusan létrejön a `04_WebAPI/Data/oceanic_dreams.db` SQLite adatbázis. Az API 4 végpontja:

| Módszer | URL | Funkció |
|---|---|---|
| GET | `/api/berlesek` | összes bérlés |
| GET | `/api/berlesek/{id}` | adott bérlés |
| POST | `/api/berlesek` | új bérlés (201 vagy 400) |
| DELETE | `/api/berlesek/{id}` | törlés (204 vagy 404) |

**POST JSON példa:** a kezdő dátum legyen **holnap vagy későbbi**, az időtartam **5–90 nap**.

```json
{
  "uid": 101,
  "yachtId": 7,
  "startDate": "2027-04-10",
  "endDate": "2027-04-14",
  "dailyPrice": 10375,
  "deposit": 15000
}
```

Az `id` az adatbázisban automatikus; a `totalPrice` számított, **[NotMapped]**. Átfedő foglalásokat ugyanarra a yacht ID-ra nem enged, de más yachtokra azonos időszak megengedett. Az `04_WebAPI/requests.http` használható VS Code REST Client bővítménnyel vagy Visual Studio HTTP-fájl támogatással.

## 05 – Foglalási frontend (15 pont)

**Előbb indítsd el a 04-es API-t**, majd egy másik terminálból:

```powershell
cd .\05_Frontend
npm start
```
## Ellenőrzések

```powershell
python .\tesztek\adatellenorzes.py
cd .\05_Frontend
npm test
```

Az API elindítása után (a csomag gyökeréből):

```powershell
python .\tesztek\api_smoke_test.py
```

Ez ideiglenes foglalást készít, lekérdezi, ellenőrzi a szabályokat, majd törli. **A tényleges C# fordítást és a Windows Forms megjelenését a saját Windows-gépen ellenőrizni kell.**

```powershell
git init
git add .
git commit -m "Oceanic Dreams yacht rental assignment"
git branch -M main
git remote add origin https://github.com/SAJAT_FELHASZNALO/SAJAT_REPO.git
git push -u origin main
```

