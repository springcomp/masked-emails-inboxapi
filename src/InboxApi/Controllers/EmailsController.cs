using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

[Authorize]
[ApiController]
[Route("[controller]")]
public sealed class EmailsController : ControllerBase
{
    private readonly IInboxService inboxService_;
    private readonly ILogger<EmailsController> logger_;

    public EmailsController(IInboxService inboxService, ILogger<EmailsController> logger)
    {
        inboxService_ = inboxService;
        logger_ = logger;
    }

    [Route("")]
    [HttpPost]
    public async Task<ActionResult> GetEmails([FromBody] string[] inboxes)
    {
        var results = new List<InboxMessageSpec>();
        foreach (var inbox in inboxes)
        {
            await foreach (var message in inboxService_.GetMessagesAsync(inbox))
                results.Add(message);
        }

        return Ok(results.ToArray());
    }

    [Route("body")]
    [HttpPost]
    public async Task<ActionResult> GetEmail()
    {
        var mediaType = MediaTypeHeaderValue.Parse(Request.ContentType);

        logger_.LogDebug($"Requesting message (content-type = '{mediaType}'.");

        var path = "";

        if (mediaType.MediaType == "application/json")
            path = await Request.Body.ReadAsJsonAsync();
        else if (mediaType.MediaType == "text/plain")
            path = await Request.Body.ReadAsStringAsync();
        else
            return StatusCode((int)HttpStatusCode.UnsupportedMediaType);

        if (string.IsNullOrWhiteSpace(path))
            return BadRequest();

        logger_.LogDebug($"Retrieving message at location '{path}'.");

        var message = await inboxService_.GetMessageAsync(path);
        return Ok(message);
    }
}

public static class StreamExtensions
{
    public static async Task<string> ReadAsStringAsync(this Stream stream)
    {
        using (var reader = new StreamReader(stream))
            return await reader.ReadToEndAsync();
    }
    public static async Task<string> ReadAsJsonAsync(this Stream stream)
    {
        return (string)JsonSerializer.Deserialize(await ReadAsStringAsync(stream), typeof(string), (JsonSerializerOptions)null);
    }
}