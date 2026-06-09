// The AppHost IS the distributed application — services and their dependencies are declared
// here in C#. Aspire provisions the containers, wires connection strings + service discovery,
// and feeds telemetry from every resource into the dashboard. No docker-compose.

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL server (Aspire runs it as a container) + a logical database for the Catalog.
var postgres = builder.AddPostgres("postgres");
var catalogDb = postgres.AddDatabase("catalogdb");

// Catalog service. WithReference injects the "catalogdb" connection string; WaitFor holds
// startup until the database resource reports healthy (so seeding never races the container).
builder.AddProject<Projects.Catalog>("catalog")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);

builder.Build().Run();
