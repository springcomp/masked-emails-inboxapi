using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InboxApi.Interop;
using MimeKit;

namespace InboxApi
{
    public class MailDirMessage : IMailDirMessage
    {
        private readonly IFilesystem filesystem_;

        public MailDirMessage(IFilesystem filesystem, string path)
        {
            filesystem_ = filesystem;
            Location = path;
        }

        public string Location { get; }

        public IDictionary<string, string[]> Headers { get; private set; }

        public string RawBody { get; private set; }

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

            // create case insensitive headers dictionary

            var headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var (key, value) in dictionary)
                headers.Add(key, value.ToArray());

            Headers = headers;

            // load message body

            RawBody = message.TextBody;
        }

        private async Task<MimeMessage> LoadMessageAsync()
        {
            await using var stream = await filesystem_.ReadToEndAsync(Location);
            return await MimeMessage.LoadAsync(stream);
        }
    }
}