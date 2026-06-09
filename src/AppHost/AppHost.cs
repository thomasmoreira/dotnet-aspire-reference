// The AppHost IS the distributed application — services and their dependencies are declared
// here in C#. Aspire provisions the containers, wires connection strings + service discovery,
// and feeds telemetry from every resource into the dashboard. No docker-compose.

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL server (Aspire runs it as a container) + a logical database for the Catalog.
var postgres = builder.AddPostgres("postgres");
var catalogDb = postgres.AddDatabase("catalogdb");

// Redis cache (also a container) for the Catalog's read-through product lookups.
var cache = builder.AddRedis("cache");

// Catalog service. WithReference injects the connection strings; WaitFor holds startup until
// each dependency reports healthy (so seeding never races the Postgres container).
builder.AddProject<Projects.Catalog>("catalog")
    .WithReference(catalogDb)
    .WithReference(cache)
    .WaitFor(catalogDb)
    .WaitFor(cache);

builder.Build().Run();
