using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace Client
{
    // http://docs.identityserver.io/en/latest/quickstarts/1_client_credentials.html

    class Program
    {
        static void Main(string[] args)
            => MainAsync(args)
                .GetAwaiter()
                .GetResult();

        static async Task MainAsync(string[] args)
        {
            var application =
                CreateDefaultBuilder(args)
                .Build()
                ;

            var serviceProvider = application.Services;
            var hostEnvironment = serviceProvider.GetService<IHostEnvironment>();

            Console.WriteLine($"Environment: {hostEnvironment.EnvironmentName}.");

            var configuration = serviceProvider.GetService<IConfiguration>();
            var section = configuration.GetSection("AppSettings");

            var server = section.GetValue<string>("server");
            var clientId = section.GetValue<string>("clientId");
            var clientSecret = section.GetValue<string>("clientSecret");

            var requestUri = section.GetValue<string>("requestUri");

            Console.WriteLine($"Identity server: {server}.");
            Console.WriteLine($"Client: {clientId}, ClientSecret: {clientSecret.Substring(0, 4)}***REDACTED***");

            const string INBOX = "alice123@domain.com";

            var tokenResponse = await RequestClientCredentials(server, clientId, clientSecret);
            if (tokenResponse == null)
            {
                Console.Error.WriteLine("An error occurred while requesting a token.");
                return;
            }

            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var inboxes = new[] { INBOX, };
            var buffer = JsonSerializer.SerializeToUtf8Bytes(inboxes);
            var requestContent = new ByteArrayContent(buffer);
            requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            var response = await client.PostAsync(requestUri, requestContent);
            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine(response.StatusCode);
            }

            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
            }
        }

        private static async Task<TokenResponse> RequestClientCredentials(string server, string clientId, string clientSecret)
        {
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(server);
            if (disco.IsError)
            {
                Console.Error.WriteLine(disco.Error);
                return null;
            }

            var tokenResponse = await client.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = clientId,
                    ClientSecret = clientSecret,

                    Scope = "inbox",
                });

            if (tokenResponse.IsError)
            {
                Console.Error.WriteLine(tokenResponse.Error);
                return null;
            }

            Console.WriteLine(tokenResponse.Json);

            return tokenResponse;
        }

        private static IHostBuilder CreateDefaultBuilder(string[] args)
        {
            // this is the same code as Host.CreateDefaultBuilder()
            // except:
            // UseContentRoot uses assembly location instead of Directory.GetDirectoryName
            // User-Secrets is used, even in non "development" environments

            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return
                new HostBuilder()
                    .UseContentRoot(location)
                    .ConfigureHostConfiguration(config =>
                    {
                        config.SetBasePath(location);
                        config.AddEnvironmentVariables("DOTNET_");

                        if (args != null)
                            config.AddCommandLine(args);
                    })
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        var hostEnvironment = context.HostingEnvironment;
                        config.AddJsonFile("appSettings.json", optional: true, reloadOnChange: true);
                        config.AddJsonFile($"appSettings.{hostEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                        if (!string.IsNullOrEmpty(hostEnvironment.ApplicationName))
                        {
                            var assembly = Assembly.Load(new AssemblyName(hostEnvironment.ApplicationName));
                            if (assembly != null)
                                config.AddUserSecrets(assembly, optional: true);
                        }

                        config.AddEnvironmentVariables();

                        if (args != null)
                            config.AddCommandLine(args);
                    })
                    .ConfigureLogging((context, logging) =>
                    {
                        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                        if (isWindows)
                        {
                            logging.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Warning);
                        }

                        logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                        logging.AddConsole();
                        logging.AddDebug();
                        logging.AddEventSourceLogger();

                        if (isWindows)
                        {
                            logging.AddEventLog();
                        }
                    })
                    .UseDefaultServiceProvider((context, options) =>
                    {
                        options.ValidateOnBuild = options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                    })
                ;
        }

    }
}

