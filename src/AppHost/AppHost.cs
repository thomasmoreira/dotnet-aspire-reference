// The AppHost IS the distributed application — services and their dependencies are declared
// here in C#. Aspire provisions the containers, wires connection strings + service discovery,
// and feeds telemetry from every resource into the dashboard. No docker-compose.

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL server (Aspire runs it as a container) + a logical database for the Catalog.
var postgres = builder.AddPostgres("postgres");
var catalogDb = postgres.AddDatabase("catalogdb");

// Redis cache (also a container) for the Catalog's read-through product lookups.
var cache = builder.AddRedis("cache");

// Catalog owns product identity/naming, backed by Postgres + Redis.
var catalog = builder.AddProject<Projects.Catalog>("catalog")
    .WithReference(catalogDb)
    .WithReference(cache)
    .WaitFor(catalogDb)
    .WaitFor(cache);

// Pricing owns the money. No backing store — deterministic demo pricing.
var pricing = builder.AddProject<Projects.Pricing>("pricing");

// Gateway (BFF) composes Catalog + Pricing. WithReference wires service discovery so the
// Gateway reaches them by name; one request fans out into a single distributed trace.
builder.AddProject<Projects.Gateway>("gateway")
    .WithReference(catalog)
    .WithReference(pricing)
    .WaitFor(catalog)
    .WaitFor(pricing);

builder.Build().Run();
