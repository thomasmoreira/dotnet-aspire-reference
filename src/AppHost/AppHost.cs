// The AppHost IS the distributed application: services and their dependencies are declared
// here in C# and Aspire orchestrates them. Resources are added per phase.

var builder = DistributedApplication.CreateBuilder(args);

builder.Build().Run();
