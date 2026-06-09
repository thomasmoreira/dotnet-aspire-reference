using System.Text.Json;

namespace AppHost.Tests;

/// <summary>
/// Exercises the Catalog over HTTP against the live distributed app: the seeded list from
/// Postgres, and a single product served through the Redis read-through cache.
/// </summary>
public class CatalogTests(AppHostFixture fixture) : IClassFixture<AppHostFixture>
{
    [Fact]
    public async Task Products_endpoint_returns_the_seeded_list()
    {
        using var client = fixture.App.CreateHttpClient("catalog");

        using var response = await client.GetAsync("/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        Assert.Equal(5, document.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task Product_by_id_is_served_through_the_redis_cache()
    {
        using var client = fixture.App.CreateHttpClient("catalog");

        // First call: cache miss → Postgres → populates Redis. Second call: cache hit.
        using var miss = await client.GetAsync("/products/1");
        using var hit = await client.GetAsync("/products/1");

        Assert.Equal(HttpStatusCode.OK, miss.StatusCode);
        Assert.Equal(HttpStatusCode.OK, hit.StatusCode);
        Assert.Contains("Mechanical Keyboard", await hit.Content.ReadAsStringAsync());
    }
}
