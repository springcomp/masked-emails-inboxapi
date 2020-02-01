using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InboxApi.Interop;
using InboxApi.Services;
using InboxApi.Tests.Impl;
using MimeKit;
using Xunit;

namespace InboxApi.Tests
{
    using FileSpec = Tuple<string, string>;
    public class MailDirTest
    {
        [Fact]
        public async Task MailDir_GetMessagePaths()
        {
            const string location = "recipient/new/1571380885.M637295P6116.mail,S=3325,W=3391";

            var filesystem = CreateInMemoryFileSystem();
            var mailDir = new MailDir(filesystem);

            var collection = mailDir.GetMessagesAsync("recipient");
            var enumerator = collection.GetAsyncEnumerator();

            var succeeded = await enumerator.MoveNextAsync();

            Assert.True(succeeded);

            var message = enumerator.Current;

            Assert.Equal(location, message.Location);

            // make sure a single entry was returned

            succeeded = await enumerator.MoveNextAsync();
            Assert.False(succeeded);
        }

        [Fact]
        public async Task MailDirMessage_Headers()
        {
            const string location = "recipient/new/1571380885.M637295P6116.mail,S=3325,W=3391";
            var filesystem = CreateInMemoryFileSystem();
            var message = new MailDirMessage(filesystem, location);

            await message.LoadAsync();

            Assert.Equal("Hello, world!", message.Headers["Subject"][0]);
            Assert.Equal("Recipient <recipient@domain.tld>", message.Headers["To"][0]);
        }

        [Fact]
        public async Task MailDirMessage_Body()
        {
            const string location = "recipient/new/1571380885.M637295P6116.mail,S=3325,W=3391";
            var filesystem = CreateInMemoryFileSystem();
            var message = new MailDirMessage(filesystem, location);

            await message.LoadAsync();

            var nl = Environment.NewLine;

            Assert.Equal($"Voici un message qui est envoyé depuis Gmail.{nl}Bien à toi.{nl}", message.RawBody);
        }

        private IFilesystem CreateInMemoryFileSystem()
        {
            return new InMemoryFileSystem(
                new FileSpec("recipient/dovecot.index.log", ""),
                new FileSpec("recipient/dovecot.list.index.log", ""),
                new FileSpec("recipient/dovecot-uidlist", ""),
                new FileSpec("recipient/dovecot-uidvalidity", ""),
                new FileSpec("recipient/new/1571380885.M637295P6116.mail,S=3325,W=3391", GetContent("recipient/1571380885.M637295P6116.mail")),
                new FileSpec("recipient/.Drafts/1571380875.M637395P6116.mail,S=3326,W=3392", GetContent("recipient/1571380885.M637295P6116.mail")),
                new FileSpec("recipient/.Junk/1571380875.M637395P6116.mail,S=3326,W=3392", GetContent("recipient/1571380885.M637295P6116.mail")),
                new FileSpec("recipient/.Sent/1571380875.M637395P6116.mail,S=3326,W=3392", GetContent("recipient/1571380885.M637295P6116.mail")),
                new FileSpec("recipient/.Trash/1571380875.M637395P6116.mail,S=3326,W=3392", GetContent("recipient/1571380885.M637295P6116.mail")),
                new FileSpec("recipient/tmp/1571380875.M637395P6116.mail,S=3326,W=3392", GetContent("recipient/1571380885.M637295P6116.mail")),
                new FileSpec("someone/dovecot.index.log", ""),
                new FileSpec("someone/dovecot.list.index.log", ""),
                new FileSpec("someone/dovecot-uidlist", ""),
                new FileSpec("someone/dovecot-uidvalidity", "")
            );
        }

        private string GetContent(string name)
        {
            return new Dictionary<string, string>{
                { "recipient/1571380885.M637295P6116.mail",
                 @"Return-Path: <medium.read@gmail.com>
Delivered-To: recipient@domain.tld
Received: from mail.domain.tld
	by mail.domain.tld with LMTP
	id 5NfdJZVeqV3kFwAAbcygvA
	(envelope-from <medium.read@gmail.com>)
	for <recipient@domain.tld>; Fri, 18 Oct 2019 06:41:25 +0000
Received: from localhost (localhost [127.0.0.1])
	by mail.domain.tld (Postfix) with ESMTP id 8E8625DD8A
	for <recipient@domain.tld>; Fri, 18 Oct 2019 06:41:25 +0000 (UTC)
Received: from mail-ot1-x335.google.com (unknown [192.168.224.1])
	by mail.domain.tld (Postfix) with ESMTPS id 3C7645DD89
	for <recipient@domain.tld>; Fri, 18 Oct 2019 06:41:25 +0000 (UTC)
Authentication-Results: mail.domain.tld;
	dkim=pass (2048-bit key; unprotected) header.d=gmail.com header.i=@gmail.com header.b=""pk3j69T6"";
	dkim-atps=neutral
Received: by mail-ot1-x335.google.com with SMTP id k32so4076067otc.4
        for <recipient@domain.tld>; Thu, 17 Oct 2019 23:41:25 -0700 (PDT)
DKIM-Signature: v=1; a=rsa-sha256; c=relaxed/relaxed;
        d=gmail.com; s=20161025;
        h=mime-version:from:date:message-id:subject:to;
        bh=I8avjerIVWDtUTcIQHho5ppbZ18I/U7Nj++PMXamA/o=;
        b=pk3j69T6ioluZY90ZbnwEz+wBhJ7x1v5F+nhNeffpakXV6FnrDVzkMi1JXAPDlxb+c
         9KRRsgfAlH0/daaCNeDCZXG68Su37xJnrtOfDFpg1BQFtoBY/C6vRAS/PxwiKsuVrCwJ
         plN6kLgV+jneKmcyTygCnpzt7BuexVwabFw3EpVXgsyURYE/ODB8Lavx8Qxq8o7lLZhk
         yBCGZcE1LSIigm9iUbKaiNYAdDIAmw/XM2Yi5hxqnyF6eCk63SjV8yIgKw1TmQ4SV1EQ
         3IFTjCSrWUPF5kGh8k/FvxfPlUhApesIWKvDrqijzs7CPWL19r846D5JgS0ykcmnHr8S
         cJ5Q==
X-Google-DKIM-Signature: v=1; a=rsa-sha256; c=relaxed/relaxed;
        d=1e100.net; s=20161025;
        h=x-gm-message-state:mime-version:from:date:message-id:subject:to;
        bh=I8avjerIVWDtUTcIQHho5ppbZ18I/U7Nj++PMXamA/o=;
        b=UNbD0GzlOv4btaG8KCfdQgVqE8/1uQtqGAJm4BPHAKpkSE5hzm8koFDalO3d7Biw+8
         6VKcxkyj9RJS1R3IXFg0TKNJj/GtCAkh5faselbrqzQwReIZvS7H6SbRjCZkfRv9Kv0v
         76WymSeMq8uANbmso3ITIO5BgYtihO66PSdXW2MLXeQZHQkYoqakD6bDyhlq+gdQZw+F
         BTcwIKJ7HyfwE+SArZpHgPlbyOwzOf5zOg0GUJVDJ8ZaDe8V1qxolqzcA9CO2+lHs3p5
         nKJ9XpsVQrr/r/AwBp7GYAEDwI8H7YIRRZkoC70KzAtkIKTXrmdhoVUO8reokcBQ9hDM
         7x+Q==
X-Gm-Message-State: APjAAAXh6xtbFnf9pwPSEx8y2YVFnPwQj5kUWoe+aI1lqaoJ7q0mREO0
	bBC+k/bQXZw48bOqqrCqL0+6zUHoZEi0Y6UXOQgevg==
X-Google-Smtp-Source: APXvYqyFUTQDXnfGXYg0jZl8W4w4liRhCJJVG02eSOtEZu4f3i8KvcAAC5Tr/SUWsDI3NsbOhg1LTXM6gKp1vCs825Y=
X-Received: by 2002:a05:6830:18e4:: with SMTP id d4mr6433731otf.171.1571380883655;
 Thu, 17 Oct 2019 23:41:23 -0700 (PDT)
MIME-Version: 1.0
From: Reader Medium <medium.read@gmail.com>
Date: Fri, 18 Oct 2019 08:41:13 +0200
Message-ID: <CADJFppBzs_7inVFm8LEwGiFoR-_J757pdFCMdkoRY20kWW2ofQ@mail.gmail.com>
Subject: Hello, world!
To: Recipient <recipient@domain.tld>
Content-Type: multipart/alternative; boundary=""0000000000006092fd0595299f5b""

--0000000000006092fd0595299f5b
Content-Type: text/plain; charset=""UTF-8""
Content-Transfer-Encoding: quoted-printable

Voici un message qui est envoy=C3=A9 depuis Gmail.
Bien =C3=A0 toi.

--0000000000006092fd0595299f5b
Content-Type: text/html; charset=""UTF-8""
Content-Transfer-Encoding: quoted-printable

<div dir=3D""ltr"">Voici un message qui est envoy=C3=A9 depuis Gmail.<div>Bie=
n =C3=A0 toi.</div></div>

--0000000000006092fd0595299f5b--"
                }
            }
            [name];
        }
    }
}