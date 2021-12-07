using InboxApi.Interop;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Utils.Owin;

#if DEBUG

Console.WriteLine("**Note**: when debugging locally, you might want to listen on an alternate tcp port.");
Console.WriteLine("Please, run 'dotnet run -- urls=\"http://localhost:5002\"' to debug locally.");

#endif

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration)
    ;

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    });
}

builder.Services
    .AddMvcCore(options => { options.EnableEndpointRouting = false; })
    .AddAuthorization()
    ;

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
app.UseAuthorization();
app.MapControllers();

app.Run();
