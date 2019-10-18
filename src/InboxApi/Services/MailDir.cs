using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InboxApi.Interop;
using InboxApi.Utils;

namespace InboxApi.Services
{
    public class MailDir : IMailDir
    {
        const string MailDirMessagePattern = @"(?:[1-9][0-9]{9})\..+\.mail,S=.*$";

        private static readonly Regex mailDirRegex_ = new Regex(MailDirMessagePattern, RegexOptions.Singleline | RegexOptions.Compiled);

        private readonly IFilesystem filesystem_;

        public MailDir(IFilesystem filesystem)
        {
            filesystem_ = filesystem;
        }

        public async IAsyncEnumerable<IMailDirMessage> GetMessagesAsync(string address)
        {
            var (_, inbox) = EmailAddressUtils.Split(address);
            foreach (var path in filesystem_.EnumerateFiles(inbox).Where(p => mailDirRegex_.Match(p).Success))
            {
                var message = new MailDirMessage(filesystem_, path);
                await message.LoadAsync();
                yield return message;
            }
        }

        public async Task<IMailDirMessage> GetMessageAsync(string path)
        {
            var message = new MailDirMessage(filesystem_, path);
            await message.LoadAsync();

            return message;
        }
    }
}