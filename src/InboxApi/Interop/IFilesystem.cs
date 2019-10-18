using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace InboxApi.Interop
{
    public interface IFilesystem
    {
        /// <summary>
        /// Recursively enumerate all files (not directories)
        /// in the directory referred to by the specified relative path.
        /// </summary>
        IEnumerable<string> EnumerateFiles(string path);

        /// <summary>
        /// Reads the content of the file referred to by the specified relative path.
        /// </summary>
        Task<Stream> ReadToEndAsync(string path);
    }
}