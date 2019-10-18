using System.Collections.Generic;
using System.Threading.Tasks;

namespace InboxApi.Interop
{
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
        /// The RFC822 message body.
        /// Initially empty, load with the <see cref="LoadAsync" /> method.
        /// </summary>
        string RawBody { get; }

        /// <summary>
        /// Reads the content of the mime message.
        /// </summary>
        /// <returns></returns>
        Task LoadAsync();
    }
}