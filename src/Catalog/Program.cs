using Catalog;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Service discovery, resilience, health checks and OpenTelemetry — shared across every service.
builder.AddServiceDefaults();

// Aspire Npgsql integration: registers an NpgsqlDataSource wired to the "catalogdb" connection
// string (injected by the AppHost), with health checks and OpenTelemetry tracing built in.
builder.AddNpgsqlDataSource("catalogdb");

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

app.MapGet("/products/{id:int}", async (int id, NpgsqlDataSource db, CancellationToken ct) =>
{
    await using var command = db.CreateCommand("SELECT id, name, sku FROM products WHERE id = $1");
    command.Parameters.Add(new NpgsqlParameter { Value = id });
    await using var reader = await command.ExecuteReaderAsync(ct);

    return await reader.ReadAsync(ct)
        ? Results.Ok(new Product(reader.GetInt32(0), reader.GetString(1), reader.GetString(2)))
        : Results.NotFound();
});

// Ensure schema + demo data before serving traffic.
await app.Services.SeedAsync();

app.Run();
