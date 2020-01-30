using InboxApi.Interop;
using InboxApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Utils.Configuration;
using Utils.Owin;

namespace InboxApi
{
    public class Startup
    {
        IWebHostEnvironment Environment { get; }
        IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore(options => { options.EnableEndpointRouting = false; })
                .AddAuthorization()
                ;

            ConfigureApplication(services);
            ConfigureDependencies(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.HandleExceptions();
            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseMvc();
        }

        private void ConfigureApplication(IServiceCollection services)
        {
            var appSettings = new AppSettings();
            var appSettingsSection = Configuration.GetSection("AppSettings");
            appSettingsSection.Bind(appSettings);

            services.AddOptions();
            services.Configure<AppSettings>(appSettingsSection);

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = appSettings.Authority;
                    options.RequireHttpsMetadata = appSettings.RequireHttpsMetadata;
                    options.Audience = appSettings.Audience;
                });

            if (!Environment.IsDevelopment())
            {
                services.AddHttpsRedirection(options =>
                {
                    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                    options.HttpsPort = appSettings.HttpsPort;
                });
            }
        }

        private void ConfigureDependencies(IServiceCollection services)
        {
            services.AddTransient<IInboxService, InboxService>();
            services.AddTransient<IMailDir, MailDir>();
            services.AddTransient<IFilesystem, Filesystem>(
                serviceProvider => new Filesystem(serviceProvider.GetService<IWebHostEnvironment>(), Configuration));
        }
    }
}
