using System.Text.RegularExpressions;
using InboxApi.Interop;

public class MailDir : IMailDir
{
    const string MailDirMessagePattern = @"(?:[1-9][0-9]{9})\..+\.mail(?:\.[^,]+)?,S=.*$";

    private static readonly Regex mailDirRegex_ = new Regex(MailDirMessagePattern, RegexOptions.Singleline | RegexOptions.Compiled);

    private readonly IFilesystem filesystem_;

    public MailDir(IFilesystem filesystem)
    {
        filesystem_ = filesystem;
    }

    public async IAsyncEnumerable<IMailDirMessage> GetMessagesAsync(string address)
    {
        var (_, inbox) = EmailAddressUtils.Split(address);
        foreach (var path in filesystem_.EnumerateFiles(inbox)
            .Where(p => IsValidSubfolder(p))
            .Where(p => mailDirRegex_.Match(p).Success))
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

    private bool IsValidSubfolder(string path)
    {
        // path is: recipient/<file>
        //      or: recipient/<folder>/<...> 

        // omit the following well-known "secondary" folders:
        // .Drafts, .Junk, .Sent, .Trash and tmp

        var excludeFolders = new[]{
                ".Drafts",
                ".Junk",
                ".Sent",
                ".Trash",
                "tmp",
            };

        var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, };
        var fragments = path.Split(separators, StringSplitOptions.None);

        if (fragments.Length > 2 && excludeFolders.Contains(fragments[1]))
            return false;

        return true;
    }

}