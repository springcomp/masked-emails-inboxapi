using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var appSettingsSection = builder.Configuration.GetSection("AppSettings");

builder.Services.Configure<AppSettings>(appSettingsSection);
builder.Services.AddTransient<Application>();

if (args.Length == 0 || args[0] != "--no-secrets")
{
    if (!string.IsNullOrEmpty(builder.Environment.ApplicationName))
    {
        var assembly = Assembly.Load(new AssemblyName(builder.Environment.ApplicationName));
        if (assembly != null)
            builder.Configuration.AddUserSecrets(assembly, optional: true);
    }
}

var host = builder.Build();

await host.Services
    .GetRequiredService<Application>()
    .RunAsync(args)
    ;
