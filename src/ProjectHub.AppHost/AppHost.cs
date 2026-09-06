var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ProjectHub_Web>("projecthub-web");

builder.Build().Run();
