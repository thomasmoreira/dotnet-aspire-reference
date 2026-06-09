using Pricing;

var builder = WebApplication.CreateBuilder(args);

// Service discovery, resilience, health checks and OpenTelemetry — shared across every service.
builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/price/{id:int}", (int id) =>
{
    // Deterministic demo pricing: a base price plus a per-id increment. Pricing owns the money,
    // Catalog owns identity/naming — the Gateway composes both into one storefront view.
    var amount = 49.90m + (id * 10m);
    return Results.Ok(new Price(id, amount, "BRL"));
});

app.Run();
