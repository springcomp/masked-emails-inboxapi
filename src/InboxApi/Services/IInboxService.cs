using System.Collections.Generic;
using System.Threading.Tasks;
using InboxApi.Model;

namespace InboxApi.Interop
{
    public interface IInboxService
    {
        IAsyncEnumerable<InboxMessageSpec> GetMessagesAsync(string address);
        Task<InboxMessage> GetMessageAsync(string path);
    }
}