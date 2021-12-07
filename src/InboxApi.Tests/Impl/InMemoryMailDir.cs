using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils;

namespace InboxApi.Tests.Impl
{
    public sealed class InMemoryMailDir : IMailDir
    {
        private readonly IList<Message> messages_
            = new List<Message>();

        public InMemoryMailDir(params Tuple<IDictionary<string, string[]>, string>[] messages)
        {
            foreach (var message in messages)
                messages_.Add(new Message(message.Item1, message.Item2));
        }

        public async IAsyncEnumerable<IMailDirMessage> GetMessagesAsync(string inbox)
        {
            foreach (var message in messages_.Where(m => m.IsRecipient(inbox)))
                yield return await Task.FromResult(message);
        }

        public Task<IMailDirMessage> GetMessageAsync(string path)
        {
            IMailDirMessage message = messages_.SingleOrDefault(m => m.Location == path);
            return Task.FromResult(message);
        }

        internal sealed class Message : IMailDirMessage
        {
            private readonly IDictionary<string, string[]> headers_;

            public Message(IDictionary<string, string[]> headers, string body)
            {
                RawBody = body;
                headers_ = headers;

                Location = GetLocation(headers["To"][0], headers["Date"][0]);
            }

            public string Location { get; private set; }
            public IDictionary<string, string[]> Headers => headers_;

            public string RawBody { get; }
            public string HtmlBody { get; }

            public Task LoadAsync()
            {
                return Task.CompletedTask;
            }

            public bool IsRecipient(string inbox)
            {
                System.Diagnostics.Debug.Assert(headers_.ContainsKey("To"));
                System.Diagnostics.Debug.Assert(headers_["To"].Length > 0);

                var recipient = headers_["To"][0];
                var succeeded = MailboxAddress.TryParse(recipient, out var to);

                var (actualDomain, actualInbox) = EmailAddressUtils.Split(to.Address);
                return inbox == actualInbox;
            }
            private static string GetLocation(string recipient, string mimeDate)
            {
                if (!MimeKit.MailboxAddress.TryParse(recipient, out var mailboxAddress))
                    throw new ArgumentException("Invalid MIME email address");

                if (!MimeKit.Utils.DateUtils.TryParse(mimeDate, out var dateTime))
                    throw new ArgumentException("Invalid MIME date/time format");

                var (domain, inbox) = EmailAddressUtils.Split(mailboxAddress.Address);
                var timestamp = UnixTime.GetTimestamp(dateTime.UtcDateTime);

                return $"{domain}/{inbox}/new/{timestamp}.M637295P6116.mail,S=3325,W=3391";
            }
        }
    }
}