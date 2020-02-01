using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InboxApi.Interop;

namespace InboxApi.Tests.Impl
{
    internal class InMemoryFileSystem : IFilesystem
    {
        private readonly IList<Tuple<string, string>> files_
        = new List<Tuple<string, string>>();

        public InMemoryFileSystem(params Tuple<string, string>[] files)
        {
            foreach (var file in files)
                files_.Add(file);
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            return files_.Where(f => f.Item1.Contains(path)).Select(f => f.Item1);
        }

        public Task<Stream> ReadToEndAsync(string path)
        {
            var content =  files_.Single(f => f.Item1 == path).Item2;
            var buffer = Encoding.UTF8.GetBytes(content);
            Stream stream = new MemoryStream(buffer);
            return Task.FromResult(stream);
        }
    }
}