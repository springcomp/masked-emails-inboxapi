using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IdentityModel.Client;

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
            var server = "http://localhost:5000";
            var clientId = "client.svc";
            var clientSecret = "please-type-the-secret-here";
            var requestUri = "http://localhost:5002/emails";

            const string INBOX = "alice123@domain.com";

            if (args.Length == 2)
            {
                clientId = args[0];
                clientSecret = args[1];
            }
            if (args.Length >= 3)
            {
                server = args[0];
                clientId = args[1];
                clientSecret = args[2];

                if (args.Length == 4)
                    requestUri = args[3];
            }
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
    }
}

