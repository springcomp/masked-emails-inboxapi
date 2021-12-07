using MimeKit;

public sealed class InboxService : IInboxService
{
    private readonly IMailDir mailDir_;

    public InboxService(IMailDir mailDir)
    {
        mailDir_ = mailDir;
    }

    public async IAsyncEnumerable<InboxMessageSpec> GetMessagesAsync(string address)
    {
        var (_, inbox) = EmailAddressUtils.Split(address);
        await foreach (var message in mailDir_.GetMessagesAsync(inbox))
            yield return MimeParse(message);
    }

    public async Task<InboxMessage> GetMessageAsync(string path)
    {
        var message = await mailDir_.GetMessageAsync(path);
        if (message.RawBody == null && message.HtmlBody == null)
            throw new KeyNotFoundException("This message may have expired or been forwarded to its target email address.");

        return MimeParse(message);
    }

    private static InboxMessage MimeParse(IMailDirMessage message)
    {
        System.Diagnostics.Debug.Assert(message.Headers.ContainsKey("From"));
        System.Diagnostics.Debug.Assert(message.Headers.ContainsKey("Date"));

        var mimeSender = message.Headers["From"][0];
        var mimeDate = message.Headers["Date"][0];

        var subject = message.Headers.ContainsKey("Subject")
                ? message.Headers["Subject"]?[0] ?? ""
                : ""
            ;

        if (!MailboxAddress.TryParse(mimeSender, out var sender))
            throw new ArgumentException("Invalid mime sender address");

        if (!MimeKit.Utils.DateUtils.TryParse(mimeDate, out var dateTimeReceived))
            throw new ArgumentException("Invalid mime date/time format");

        return new InboxMessage
        {
            Location = message.Location,
            Sender = new EmailAddress { DisplayName = sender.Name, Address = sender.Address, },
            ReceivedUtc = dateTimeReceived.UtcDateTime,
            Subject = subject,

            TextBody = message.RawBody,
            HtmlBody = message.HtmlBody,
        };
    }
}