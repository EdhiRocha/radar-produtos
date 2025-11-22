using Microsoft.EntityFrameworkCore;
using RadarProdutos.Api.Middleware;
using RadarProdutos.Application.Services;
using RadarProdutos.Domain.Interfaces;
using RadarProdutos.Infrastructure.Data;
using RadarProdutos.Infrastructure.ExternalServices;
using RadarProdutos.Infrastructure.Repositories;
using RadarProdutos.Infrastructure.Scraper;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// DbContext - PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var logger = builder.Logging.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>().CreateLogger("Program");

if (string.IsNullOrEmpty(connectionString))
{
    // Fallback para InMemory se não tiver connection string configurada
    logger.LogWarning("Connection string não encontrada. Usando InMemory Database");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("RadarDb"));
}
else
{
    logger.LogInformation("Conectando ao PostgreSQL: {ConnectionInfo}", connectionString.Split(';')[0]);
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductAnalysisRepository, ProductAnalysisRepository>();
builder.Services.AddScoped<IAnalysisConfigRepository, AnalysisConfigRepository>();
builder.Services.AddScoped<IMarketplaceConfigRepository, MarketplaceConfigRepository>();
builder.Services.AddScoped<IShippingEstimateRepository, ShippingEstimateRepository>();

// Application services
builder.Services.AddScoped<IAnalysisService, AnalysisService>();
builder.Services.AddScoped<IHotProductsService, HotProductsService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

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
    var seedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Aplicar migrations automaticamente
    try
    {
        db.Database.Migrate();
        seedLogger.LogInformation("Migrations aplicadas com sucesso");
    }
    catch (Exception ex)
    {
        seedLogger.LogWarning(ex, "Erro ao aplicar migrations");
        // Se for InMemory, migrations não funcionam, apenas seguir em frente
    }

    // Seed config removido - agora vem das migrations
}

// Exception Handling Middleware (DEVE vir antes de qualquer outro middleware)
app.UseExceptionHandling();

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

app.Run();
