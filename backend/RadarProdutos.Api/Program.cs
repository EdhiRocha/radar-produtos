using Microsoft.EntityFrameworkCore;
using RadarProdutos.Application.Services;
using RadarProdutos.Domain.Interfaces;
using RadarProdutos.Infrastructure.Data;
using RadarProdutos.Infrastructure.ExternalServices;
using RadarProdutos.Infrastructure.Repositories;
using RadarProdutos.Infrastructure.Scraper;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// DbContext - PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // Fallback para InMemory se não tiver connection string configurada
    Console.WriteLine("⚠️  Connection string não encontrada. Usando InMemory Database.");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("RadarDb"));
}
else
{
    Console.WriteLine($"✅ Conectando ao PostgreSQL: {connectionString.Split(';')[0]}");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductAnalysisRepository, ProductAnalysisRepository>();
builder.Services.AddScoped<IAnalysisConfigRepository, AnalysisConfigRepository>();

// Application services
builder.Services.AddScoped<IAnalysisService, AnalysisService>();

// HttpClient typed client for scraper (reads base url from configuration)
// AliExpress API Client
builder.Services.AddHttpClient<IAliExpressClient, AliExpressClient>();
builder.Services.AddScoped<IScraperClient, ScraperHttpClient>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed default config and plans
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Aplicar migrations automaticamente
    try
    {
        db.Database.Migrate();
        Console.WriteLine("✅ Migrations aplicadas com sucesso");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Erro ao aplicar migrations: {ex.Message}");
        // Se for InMemory, migrations não funcionam, apenas seguir em frente
    }

    // Seed config
    if (!db.AnalysisConfigs.Any())
    {
        db.AnalysisConfigs.Add(new RadarProdutos.Domain.Entities.AnalysisConfig
        {
            Id = 1,
            MinMarginPercent = 10,
            MaxMarginPercent = 60,
            WeightSales = 1,
            WeightCompetition = 1,
            WeightSentiment = 1,
            WeightMargin = 1
        });
        db.SaveChanges();
        Console.WriteLine("✅ AnalysisConfig seed criado");
    }
}

// Swagger habilitado em todos os ambientes
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapControllers();

// Garantir que sempre use a porta 5001
app.Urls.Clear();
app.Urls.Add("http://localhost:5001");

app.Run();
