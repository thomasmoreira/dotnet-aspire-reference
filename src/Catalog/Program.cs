using System.Text.Json;
using Catalog;
using Npgsql;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Service discovery, resilience, health checks and OpenTelemetry — shared across every service.
builder.AddServiceDefaults();

// Aspire integrations: NpgsqlDataSource + Redis connection, each wired to the connection string
// injected by the AppHost, with health checks and OpenTelemetry instrumentation built in.
builder.AddNpgsqlDataSource("catalogdb");
builder.AddRedisClient("cache");

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/products", async (NpgsqlDataSource db, CancellationToken ct) =>
{
    var products = new List<Product>();
    await using var command = db.CreateCommand("SELECT id, name, sku FROM products ORDER BY id");
    await using var reader = await command.ExecuteReaderAsync(ct);
    while (await reader.ReadAsync(ct))
    {
        products.Add(new Product(reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
    }

    return Results.Ok(products);
});

// Read-through cache: try Redis first; on a miss, hit Postgres and populate the cache. Both the
// cache lookup and the DB query show up as nested spans in the distributed trace.
app.MapGet("/products/{id:int}", async (int id, NpgsqlDataSource db, IConnectionMultiplexer redis, CancellationToken ct) =>
{
    var cache = redis.GetDatabase();
    var cacheKey = $"product:{id}";

    var cached = await cache.StringGetAsync(cacheKey);
    if (cached.HasValue)
    {
        return Results.Content(cached!, "application/json");
    }

    await using var command = db.CreateCommand("SELECT id, name, sku FROM products WHERE id = $1");
    command.Parameters.Add(new NpgsqlParameter { Value = id });
    await using var reader = await command.ExecuteReaderAsync(ct);
    if (!await reader.ReadAsync(ct))
    {
        return Results.NotFound();
    }

    var product = new Product(reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
    var json = JsonSerializer.Serialize(product, JsonSerializerOptions.Web);
    await cache.StringSetAsync(cacheKey, json, TimeSpan.FromMinutes(5));

    return Results.Content(json, "application/json");
});

// Ensure schema + demo data before serving traffic.
await app.Services.SeedAsync();

app.Run();
