var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.RooviApp_Identity_Api>("rooviapp-identity-api");

builder.Build().Run();
