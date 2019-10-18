using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InboxApi.Interop;
using InboxApi.Tests.Impl;
using Xunit;

namespace InboxApi.Tests
{
    using InboxApiTestMessage = Tuple<IDictionary<string, string[]>, string>;

    public class InboxServiceTests
    {
        [Fact]
        public async Task InboxService_ListMessages()
        {
            var mailDir = CreateInMemoryMailDir();
            var service = new InboxService(mailDir);

            var collection = service.GetMessagesAsync("recipient@domain.tld");
            var enumerator = collection.GetAsyncEnumerator();
            var succeeded = await enumerator.MoveNextAsync();

            Assert.True(succeeded);

            var message = enumerator.Current;

            Assert.Equal("no-reply@example.com", message.Sender.Address);
            Assert.Equal("subject", message.Subject);
            Assert.Equal(new DateTime(2019, 10, 18, 07, 41, 43, DateTimeKind.Utc), message.ReceivedUtc);

            // make sure there was only one record returned

            succeeded = await enumerator.MoveNextAsync();

            Assert.False(succeeded);
        }

        [Fact]
        public async Task InboxService_GetMessage()
        {
            var mailDir = CreateInMemoryMailDir();
            var service = new InboxService(mailDir);

            var message = await service.GetMessageAsync("domain.tld/recipient/new/1571384503.M637295P6116.mail,S=3325,W=3391");

            Assert.Equal("Voici un message qui est envoyé depuis Gmail.\r\nBien à toi.\r\n", message.TextBody);
        }

        private static IMailDir CreateInMemoryMailDir()
        {
            return new InMemoryMailDir(
                new InboxApiTestMessage(
                    new Dictionary<string, string[]>
                    {
                        { "From", new string[]{ "Sender <no-reply@example.com>"} },
                        { "Subject", new []{ "subject", } },
                        { "Date", new string[]{ "Fri, 18 Oct 2019 09:41:43 +0200"} },
                        { "To", new string[]{ "Someone <someone@domain.tld>"} },
                        { "...", new string[]{} },
                    }
                    , "Hello, world!"
                ),
                new InboxApiTestMessage(
                    new Dictionary<string, string[]>
                    {
                        { "From", new string[]{ "Sender <no-reply@example.com>"} },
                        { "Subject", new []{ "subject", } },
                        { "Date", new string[]{ "Fri, 18 Oct 2019 09:41:43 +0200"} },
                        { "To", new string[]{ "Recipient <recipient@domain.tld>"} },
                        { "...", new string[]{} },
                    }
                    , "Voici un message qui est envoyé depuis Gmail.\r\nBien à toi.\r\n"
                )
            );
        }
    }
}