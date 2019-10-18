using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InboxApi.Interop;
using Microsoft.Extensions.Configuration;

namespace InboxApi.Services
{
    internal sealed class Filesystem : IFilesystem
    {
        private readonly string rootPath_;

        public Filesystem(IConfiguration configuration)
            : this(configuration["Inbox:Root"])
        { }

        public Filesystem(string rootPath)
        {
            rootPath_ = rootPath;
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            var fullPath = GetFullPath(path);
            return Directory
                    .EnumerateFiles(fullPath, "*.*", SearchOption.AllDirectories)
                    .Select(MakeRelativePath)
                ;
        }

        public async Task<Stream> ReadToEndAsync(string path)
        {
            var fullPath = GetFullPath(path);
            var buffer = await File.ReadAllBytesAsync(fullPath);
            return new MemoryStream(buffer);
        }

        private string GetFullPath(string path)
        {
            var fullPath = Path.Combine(rootPath_, path);
            return fullPath;
        }
        private string MakeRelativePath(string fullPath)
        {
            var relativePath = fullPath.Replace(rootPath_, "");
            if (relativePath.StartsWith(Path.DirectorySeparatorChar))
                relativePath = relativePath.Substring(1);

            return relativePath;
        }

    }
}