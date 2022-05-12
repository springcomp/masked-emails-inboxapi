using InboxApi.Interop;
using MimeKit;
using System.Text.RegularExpressions;

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
        HtmlBody = message.HtmlBody != null
            ? ParseHtml(message)
            : ""
            ;
    }

    private async Task<MimeMessage> LoadMessageAsync()
    {
        await using var stream = await filesystem_.ReadToEndAsync(Location);
        if (stream == Stream.Null)
            return Empty;

        return await MimeMessage.LoadAsync(stream);
    }

    private static readonly Regex HtmlSrcRegex =
        new Regex("<img [^\\>]*src=\"cid:(?<cid>[^\"]+)\"[^\\>]*>")
        ;
    private string ParseHtml(MimeMessage message)
    {
        return HtmlSrcRegex.Replace(
            message.HtmlBody,
            m => EmbedContentId(m, message)
            );
    }

    private static string EmbedContentId(Match match, MimeMessage message)
    {
        var text = match.Groups[0].Value;

        var cid = match.Groups["cid"].Value;
        var bodyPart = GetContentIdBodyPart(message, cid);
        if (bodyPart == null)
            return text;

        var pattern = $"src=\"cid:{cid}\"";
        var mime = bodyPart.ContentType?.MimeType ?? "image/png";
        var base64 = GetBase64Content(bodyPart);

        return text.Replace(
            pattern,
            $"src=\"data:{mime};base64,{base64}\""
            );
    }

    private static string GetBase64Content(MimePart bodyPart)
    {
        string base64;
        using (var stream = bodyPart.Content?.Open() ?? Stream.Null)
        {
            var buffer = new byte[4096];
            var count = stream.Read(buffer, 0, buffer.Length);
            base64 = Convert.ToBase64String(buffer, 0, count);
        }

        return base64;
    }

    private static MimePart GetContentIdBodyPart(MimeMessage message, string cid)
    {
        return message
            .BodyParts
            .OfType<MimePart>()
            .SingleOrDefault(
                p => p.Headers.Any(header => header.Id == HeaderId.ContentId && header.Value == $"<{cid}>")
                );
    }
}