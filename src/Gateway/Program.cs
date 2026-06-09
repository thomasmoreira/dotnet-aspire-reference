using System.Net;
using System.Net.Http.Json;
using Gateway;

var builder = WebApplication.CreateBuilder(args);

// Service discovery, resilience, health checks and OpenTelemetry — shared across every service.
builder.AddServiceDefaults();

// Typed clients addressed by service name. "https+http://catalog" is resolved by service
// discovery (ServiceDefaults) to the address the AppHost injected — no hardcoded URLs. The
// HttpClient is OpenTelemetry-instrumented, so these calls become child spans of one trace.
builder.Services.AddHttpClient("catalog", client => client.BaseAddress = new Uri("https+http://catalog"));
builder.Services.AddHttpClient("pricing", client => client.BaseAddress = new Uri("https+http://pricing"));

var app = builder.Build();

app.MapDefaultEndpoints();

// The full storefront: one call to Catalog for the list, then a price per item from Pricing.
// This fan-out shows up as a richer distributed trace (one Catalog span + N Pricing spans).
app.MapGet("/storefront", async (IHttpClientFactory factory, CancellationToken ct) =>
{
    var catalog = factory.CreateClient("catalog");
    var pricing = factory.CreateClient("pricing");

    var products = await catalog.GetFromJsonAsync<List<ProductDto>>("/products", ct) ?? [];

    var items = new List<StorefrontItem>(products.Count);
    foreach (var product in products)
    {
        var price = await pricing.GetFromJsonAsync<PriceDto>($"/price/{product.Id}", ct);
        items.Add(new StorefrontItem(
            product.Id, product.Name, product.Sku, price?.Amount ?? 0m, price?.Currency ?? "BRL"));
    }

    return Results.Ok(items);
});

// One real request → a distributed trace crossing Gateway → Catalog (→ Postgres/Redis) → Pricing.
app.MapGet("/storefront/{id:int}", async (int id, IHttpClientFactory factory, CancellationToken ct) =>
{
    var catalog = factory.CreateClient("catalog");
    var pricing = factory.CreateClient("pricing");

    using var productResponse = await catalog.GetAsync($"/products/{id}", ct);
    if (productResponse.StatusCode == HttpStatusCode.NotFound)
    {
        return Results.NotFound();
    }

    productResponse.EnsureSuccessStatusCode();
    var product = await productResponse.Content.ReadFromJsonAsync<ProductDto>(ct);

    using var priceResponse = await pricing.GetAsync($"/price/{id}", ct);
    priceResponse.EnsureSuccessStatusCode();
    var price = await priceResponse.Content.ReadFromJsonAsync<PriceDto>(ct);

    if (product is null || price is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new StorefrontItem(product.Id, product.Name, product.Sku, price.Amount, price.Currency));
});

app.Run();
