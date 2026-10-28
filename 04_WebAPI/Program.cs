using Microsoft.EntityFrameworkCore;
using OceanicApi.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddCors(options => options.AddPolicy("frontend", policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

string dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "Data");
Directory.CreateDirectory(dataDirectory);
string databasePath = Path.Combine(dataDirectory, "oceanic_dreams.db");
builder.Services.AddDbContext<OceanicContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OceanicContext>();
    context.Database.EnsureCreated();
}
app.UseCors("frontend");
app.MapControllers();
app.Run();
