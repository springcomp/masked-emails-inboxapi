using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InboxApi.Interop;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace InboxApi.Services
{
    internal sealed class Filesystem : IFilesystem
    {
        private readonly string rootPath_;

        public Filesystem(IWebHostEnvironment environment, IConfiguration configuration)
            : this(environment, configuration["Inbox:Root"])
        {
        }

        public Filesystem(IWebHostEnvironment environment, string rootPath)
        {
            rootPath_ = rootPath.Replace("~", environment.ContentRootPath);
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            var fullPath = GetFullPath(path);
            try
            {
                return Directory
                        .EnumerateFiles(fullPath, "*.*", SearchOption.AllDirectories)
                        .Select(MakeRelativePath)
                    ;
            }
            catch (DirectoryNotFoundException)
            {
                // mailboxes may disappear

                return Enumerable.Empty<string>();
            }
        }

        public async Task<Stream> ReadToEndAsync(string path)
        {
            var fullPath = GetFullPath(path);
            try
            {
                var buffer = await File.ReadAllBytesAsync(fullPath);
                return new MemoryStream(buffer);
            }
            catch (FileNotFoundException)
            {
                // messages may disappear
                // - forwarded to target email address
                // - purged after expiration time
                // - mailbox removed

                return Stream.Null;
            }
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