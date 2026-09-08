using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var useRemoteDb = builder.Configuration.GetValue<bool>("UseRemoteDatabase");

var redis = builder.AddRedis("redis")
    .WithRedisCommander()
    .WithRedisInsight();

//Do we want to do additional overides, or do se use a locally configured DB
if (!useRemoteDb)
{
    // Latest SQL, Persistent to ensure faster startup, and a named DB
    // NOTE: Persistent DB here DOES ensure that a DB is re-used across runs, but it does not ensure that the DB is cleaned up between runs. If you want a clean DB each time, use ContainerLifetime.Transient instead of Persistent.
    var sql = builder.AddSqlServer("sql")
        .WithImageTag("2025-latest")
        .WithLifetime(ContainerLifetime.Persistent)
        .AddDatabase("DefaultConnection", "ProjectHubDb2026");

    // Run the migrations, with a reference to SQL so the connection information is injected, and wait for SQL to be ready
    var migrations = builder.AddProject<Projects.ProjectHub_AppHostSetupWorker>("Migrations")
        .WithReference(sql)
        .WaitFor(sql);

    // Lastly add the Web project, with references to both SQL and Migrations, and wait for both to be ready before starting the Web project
    builder.AddProject<Projects.ProjectHub_Web>("projecthub-web")
        .WithReference(sql)
        .WithReference(migrations)
        .WithReference(redis)
        .WaitFor(sql)
        .WaitFor(migrations)
        .WaitFor(redis)
        .WithExternalHttpEndpoints();
}
else
{
    builder.AddProject<Projects.ProjectHub_Web>("web")
        .WithReference(redis)
        .WaitFor(redis)
        .WithExternalHttpEndpoints();
}

builder.Build().Run();
