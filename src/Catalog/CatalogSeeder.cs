using Npgsql;

namespace Catalog;

/// <summary>
/// Creates the products table and seeds demo rows on startup. Idempotent and resilient:
/// even with <c>WaitFor</c> in the AppHost, the first connection can race the container,
/// so it retries until Postgres accepts queries.
/// </summary>
internal static partial class CatalogSeeder
{
    private static readonly (int Id, string Name, string Sku)[] DemoProducts =
    [
        (1, "Mechanical Keyboard", "KBD-001"),
        (2, "27\" 4K Monitor", "MON-027"),
        (3, "USB-C Hub", "HUB-007"),
        (4, "Noise-cancelling Headset", "AUD-100"),
        (5, "1080p Webcam", "CAM-010"),
    ];

    public static async Task SeedAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var dataSource = services.GetRequiredService<NpgsqlDataSource>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Catalog.Seeder");

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

                await using (var create = new NpgsqlCommand(
                    "CREATE TABLE IF NOT EXISTS products (id int PRIMARY KEY, name text NOT NULL, sku text NOT NULL)",
                    connection))
                {
                    await create.ExecuteNonQueryAsync(cancellationToken);
                }

                await using var batch = new NpgsqlBatch(connection);
                foreach (var (id, name, sku) in DemoProducts)
                {
                    var command = new NpgsqlBatchCommand(
                        "INSERT INTO products (id, name, sku) VALUES ($1, $2, $3) ON CONFLICT (id) DO NOTHING");
                    command.Parameters.Add(new NpgsqlParameter { Value = id });
                    command.Parameters.Add(new NpgsqlParameter { Value = name });
                    command.Parameters.Add(new NpgsqlParameter { Value = sku });
                    batch.BatchCommands.Add(command);
                }

                await batch.ExecuteNonQueryAsync(cancellationToken);
                LogSeeded(logger, DemoProducts.Length);
                return;
            }
            catch (NpgsqlException ex) when (attempt < 15)
            {
                LogWaiting(logger, attempt, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Catalog seeded ({Count} products ensured)")]
    private static partial void LogSeeded(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Catalog database not ready (attempt {Attempt}): {Reason} — retrying")]
    private static partial void LogWaiting(ILogger logger, int attempt, string reason);
}
