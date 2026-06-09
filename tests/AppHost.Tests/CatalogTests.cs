using System.Text.Json;

namespace AppHost.Tests;

/// <summary>
/// Spins up the whole distributed app (AppHost → Postgres container + Catalog) via
/// Aspire.Hosting.Testing, then asserts the Catalog serves its seeded data over HTTP.
/// This is the lab's live verification — real containers, started and torn down cleanly.
/// </summary>
public class CatalogTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(3);

    [Fact]
    public async Task Catalog_serves_seeded_products_from_postgres()
    {
        using var cts = new CancellationTokenSource(Timeout);
        var ct = cts.Token;

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>(ct);

        await using var app = await appHost.BuildAsync(ct);
        await app.StartAsync(ct);

        await app.ResourceNotifications.WaitForResourceHealthyAsync("catalog", ct);

        using var client = app.CreateHttpClient("catalog");
        using var response = await client.GetAsync("/products", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync(ct);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(5, document.RootElement.GetArrayLength());
    }
}
