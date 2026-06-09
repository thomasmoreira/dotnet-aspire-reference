using System.Text.Json;

namespace AppHost.Tests;

/// <summary>
/// Hits the Gateway's storefront endpoint, which fans out to Catalog and Pricing over service
/// discovery. A successful composed response proves the cross-service call chain — the same
/// chain that shows up as one distributed trace in the Aspire dashboard.
/// </summary>
[Collection("aspire-app")]
public class GatewayTests(AppHostFixture fixture)
{
    [Fact]
    public async Task Storefront_composes_catalog_and_pricing()
    {
        using var client = fixture.App.CreateHttpClient("gateway");

        using var response = await client.GetAsync("/storefront/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("Mechanical Keyboard", root.GetProperty("name").GetString());   // from Catalog
        Assert.Equal("BRL", root.GetProperty("currency").GetString());               // from Pricing
        Assert.True(root.GetProperty("price").GetDecimal() > 0);                      // from Pricing
    }

    [Fact]
    public async Task Storefront_list_fans_out_to_all_products_with_prices()
    {
        using var client = fixture.App.CreateHttpClient("gateway");

        using var response = await client.GetAsync("/storefront");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        Assert.Equal(5, document.RootElement.GetArrayLength());
        foreach (var item in document.RootElement.EnumerateArray())
        {
            Assert.True(item.GetProperty("price").GetDecimal() > 0);
        }
    }

    [Fact]
    public async Task Storefront_returns_404_for_unknown_product()
    {
        using var client = fixture.App.CreateHttpClient("gateway");

        using var response = await client.GetAsync("/storefront/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
