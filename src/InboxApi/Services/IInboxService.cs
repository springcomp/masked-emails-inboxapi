public interface IInboxService
{
    IAsyncEnumerable<InboxMessageSpec> GetMessagesAsync(string address);
    Task<InboxMessage> GetMessageAsync(string path);
    Task<string> GetRawMessageAsync(string path);
}