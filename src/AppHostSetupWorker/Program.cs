using AppHostSetupWorker;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ProjectHub.Data;
using ProjectHub.Data.Models;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

//Be sure to use the proper connection string mapping here
builder.AddSqlServerDbContext<ApplicationDbContext>("DefaultConnection");

//Add identity required items so that our model's are correct (Long-Term COnsider a shared/setup Method)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var host = builder.Build();
host.Run();
