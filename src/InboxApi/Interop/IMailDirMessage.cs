/// <summary>
/// Represents a particular inbox message initialized with header metadata
/// </summary>
public interface IMailDirMessage
{

    /// <summary>
    /// The relative path of the message in the filesystem.
    /// e.g. domain.tld/inbox/new/1571380885.M637295P6116.mail,S=3325,W=3391
    /// </summary>
    string Location { get; }

    /// <summary>
    /// The RFC822 mime headers.
    /// Initially empty, load with the <see cref="LoadAsync" /> method.
    /// </summary>
    IDictionary<string, string[]> Headers { get; }

    /// <summary>
    /// The plain/text | HTML message body.
    /// Initially empty, load with the <see cref="LoadAsync" /> method.
    /// </summary>
    string TextBody { get; }
    string HtmlBody { get; }

    /// <summary>
    /// Reads the content of the mime message.
    /// </summary>
    /// <returns></returns>
    Task LoadAsync();

    /// Reads the raw RFC822 .eml message source
    Task<string> GetSourceAsync();
}