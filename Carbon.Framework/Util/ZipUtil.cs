using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using Ionic.Zlib;

namespace Carbon.Framework.Util
{
    public class ZipUtil
    {
        public class Zip : IDisposable
        {
            private readonly Stream _stream;
            private readonly ZipFile _zipFile;
            private readonly Encoding _encoding = Encoding.UTF8;

            public Zip(Stream stream, bool compress = true)
            {
                _stream = stream;
                _zipFile = new ZipFile(_encoding);
                if (!compress)
                {
                    _zipFile.CompressionLevel = CompressionLevel.None;
                }                
            }

            public void Add(string name, byte[] content)
            {                
                _zipFile.AddEntry(name, content);
            }
            public void Add(string name, string content)
            {
                _zipFile.AddEntry(name, _encoding.GetBytes(content));
            }
            public void Add(string name, Stream stream)
            {
                _zipFile.AddEntry(name, stream);
            }
            public void AddFile(string fileName, string dirPathInArchive)
            {
                _zipFile.AddFile(fileName, dirPathInArchive);
            }

            public void Dispose()
            {
                _zipFile.Save(_stream); 
                _zipFile.Dispose();
            }
        }

        public void Extract(Stream stream, string toDir)
        {
            using (var zip = ZipFile.Read(stream))
            {                                   
                zip.ExtractAll(toDir, ExtractExistingFileAction.OverwriteSilently);
            }
        }
        
        public Task SaveToStreamAsync(IDictionary<string, Stream> entries, IDictionary<string, byte[]> byteEntries, Stream output)
        {
            return Task.Run(() =>
            {
                using (var zip = new ZipFile())
                {
                    foreach (var entry in entries)
                    {
                        zip.AddEntry(entry.Key, entry.Value);
                    }
                    foreach (var entry in byteEntries)
                    {
                        zip.AddEntry(entry.Key, entry.Value);
                    }
                    zip.Save(output);
                }
            });
        }
    }
}