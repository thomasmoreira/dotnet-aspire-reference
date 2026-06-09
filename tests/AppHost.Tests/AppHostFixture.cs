using Aspire.Hosting;

namespace AppHost.Tests;

/// <summary>
/// Starts the whole distributed application once (AppHost → Postgres + Redis + Catalog +
/// Pricing + Gateway) via Aspire.Hosting.Testing and shares it across the test collection.
/// Real containers, started once and disposed cleanly — the lab's live verification harness.
/// </summary>
public sealed class AppHostFixture : IAsyncLifetime
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(3);

    public DistributedApplication App { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>(CancellationToken.None);
        App = await builder.BuildAsync();
        await App.StartAsync();

        await App.ResourceNotifications.WaitForResourceHealthyAsync("catalog").WaitAsync(StartupTimeout);
        await App.ResourceNotifications.WaitForResourceHealthyAsync("gateway").WaitAsync(StartupTimeout);
    }

    public async Task DisposeAsync() => await App.DisposeAsync();
}
