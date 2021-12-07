public class InboxMessageSpec
{
    public string Location { get; set; }
    public DateTime ReceivedUtc { get; set; }
    public string Subject { get; set; }
    public EmailAddress Sender { get; set; }
}

public class InboxMessage : InboxMessageSpec
{
    public string TextBody { get; set; }
    public string HtmlBody { get; set; }
}