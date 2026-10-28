using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OceanicApi.Data;
using OceanicApi.Models;

namespace OceanicApi.Controllers;

[ApiController]
[Route("api/berlesek")]
public sealed class BerlesekController(OceanicContext adatbazis) : ControllerBase
{
    // Egyszerre egy POST-ellenőrzés és -mentés futhat ebben a demonstrációs folyamatban.
    private static readonly SemaphoreSlim FoglalasZar = new(1, 1);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Berles>>> Osszes()
    {
        return Ok(await adatbazis.Berlesek.AsNoTracking().OrderBy(b => b.Id).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Berles>> Egy(int id)
    {
        Berles? berles = await adatbazis.Berlesek.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        return berles is null ? NotFound(new { message = "Reservation not found." }) : Ok(berles);
    }

    [HttpPost]
    public async Task<ActionResult<Berles>> Letrehozas([FromBody] BerlesLetrehozas uj)
    {
        DateTime kezdet = uj.StartDate.Date;
        DateTime vege = uj.EndDate.Date;
        if (kezdet < DateTime.Today.AddDays(1))
            return BadRequest(new { message = "The reservation must start on or after tomorrow." });

        int napok = (vege - kezdet).Days + 1;
        if (napok < 5)
            return BadRequest(new { message = "The minimum yacht rental period is 5 days." });
        if (napok > 90)
            return BadRequest(new { message = "The maximum yacht rental period is 90 days." });

        await FoglalasZar.WaitAsync();
        try
        {
            bool vanAtfedes = await adatbazis.Berlesek.AnyAsync(b =>
                b.YachtId == uj.YachtId &&
                b.StartDate <= vege &&
                b.EndDate >= kezdet);
            if (vanAtfedes)
                return BadRequest(new { message = "The selected yacht is already booked during the specified period." });

            var berles = new Berles
            {
                Uid = uj.Uid,
                YachtId = uj.YachtId,
                StartDate = kezdet,
                EndDate = vege,
                DailyPrice = uj.DailyPrice,
                Deposit = uj.Deposit
            };
            adatbazis.Berlesek.Add(berles);
            await adatbazis.SaveChangesAsync();
            return CreatedAtAction(nameof(Egy), new { id = berles.Id }, berles);
        }
        finally
        {
            FoglalasZar.Release();
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Torles(int id)
    {
        Berles? berles = await adatbazis.Berlesek.FindAsync(id);
        if (berles is null)
            return NotFound(new { message = "Reservation not found." });

        adatbazis.Berlesek.Remove(berles);
        await adatbazis.SaveChangesAsync();
        return NoContent();
    }
}
