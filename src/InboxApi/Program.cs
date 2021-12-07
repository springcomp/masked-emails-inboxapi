using InboxApi.Interop;
using Utils.Configuration;
using Utils.Owin;

#if DEBUG

Console.WriteLine("**Note**: when debugging locally, you might want to listen on an alternate tcp port.");
Console.WriteLine("Please, run 'dotnet run -- urls=\"http://localhost:5002\" to debug locally.");

#endif

var builder = WebApplication.CreateBuilder(args);

var appSettings = new AppSettings();
var appSettingsSection = builder.Configuration.GetSection("AppSettings");
appSettingsSection.Bind(appSettings);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = appSettings.Authority;
        options.RequireHttpsMetadata = appSettings.RequireHttpsMetadata;
        options.Audience = appSettings.Audience;
    });

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
        options.HttpsPort = appSettings.HttpsPort;
    });
}

builder.Services
    .AddMvcCore(options => { options.EnableEndpointRouting = false; })
    .AddAuthorization()
    ;

builder.Services.AddOptions();
builder.Services.Configure<AppSettings>(appSettingsSection);

builder.Services.AddTransient<IInboxService, InboxService>();
builder.Services.AddTransient<IMailDir, MailDir>();
builder.Services.AddTransient<IFilesystem, Filesystem>(
    serviceProvider => new Filesystem(
        serviceProvider.GetService<IWebHostEnvironment>(), builder.Configuration)
    );

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.HandleExceptions();
app.UseAuthentication();

app.UseStaticFiles();

app.UseMvc();

app.Run();
