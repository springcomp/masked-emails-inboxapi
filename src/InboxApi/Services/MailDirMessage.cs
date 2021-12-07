using InboxApi.Interop;
using MimeKit;

public class MailDirMessage : IMailDirMessage
{
    private static readonly MimeMessage Empty = new MimeMessage();

    private readonly IFilesystem filesystem_;

    public MailDirMessage(IFilesystem filesystem, string path)
    {
        filesystem_ = filesystem;
        Location = path;
    }

    public string Location { get; }

    public IDictionary<string, string[]> Headers { get; private set; }

    public string RawBody { get; private set; }
    public string HtmlBody { get; private set; }

    public async Task LoadAsync()
    {
        var message = await LoadMessageAsync();

        var dictionary = new Dictionary<string, List<string>>();
        foreach (var header in message.Headers)
        {
            if (!dictionary.ContainsKey(header.Field))
                dictionary.Add(header.Field, new List<string>());
            dictionary[header.Field].Add(header.Value);
        }

        // convert List<string> to string[]

        var headers = dictionary.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToArray()
        );

        Headers = headers;

        // load message body

        RawBody = message.TextBody;
        HtmlBody = message.HtmlBody;
    }

    private async Task<MimeMessage> LoadMessageAsync()
    {
        await using var stream = await filesystem_.ReadToEndAsync(Location);
        if (stream == Stream.Null)
            return Empty;

        return await MimeMessage.LoadAsync(stream);
    }
}