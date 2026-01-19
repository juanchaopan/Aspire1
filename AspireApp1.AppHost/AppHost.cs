var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
var db1 = postgres.AddDatabase("db-1");

builder.AddProject<Projects.WebApplication1>("webapplication1")
    .WithReference(db1)
    .WaitFor(db1);

builder.Build().Run();
