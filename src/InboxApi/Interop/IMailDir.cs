using System.Collections.Generic;
using System.Threading.Tasks;

namespace InboxApi.Interop
{
    /// <summary>
    /// Represents a filesystem-based MailDir inbox associated with a particular domain.
    /// </summary>
    public interface IMailDir
    {
        IAsyncEnumerable<IMailDirMessage> GetMessagesAsync(string inbox);
        Task<IMailDirMessage> GetMessageAsync(string path);
    }
}