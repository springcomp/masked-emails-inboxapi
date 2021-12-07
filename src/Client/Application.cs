using System.Net.Http.Headers;
using System.Text.Json;

using IdentityModel.Client;

public sealed class Application
{
    private readonly AppSettings appSettings_;

    public Application(IOptions<AppSettings> appSettings)
    {
        appSettings_ = appSettings.Value;
    }

    public async Task RunAsync(string[] args)
    {
        var server = appSettings_.Server;
        var clientId = appSettings_.ClientId;
        var clientSecret = appSettings_.ClientSecret;

        var requestUri = appSettings_.RequestUri;

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

    private static async Task<TokenResponse?> RequestClientCredentials(string server, string clientId, string clientSecret)
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
