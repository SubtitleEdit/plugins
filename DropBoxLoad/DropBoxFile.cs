using System;
using System.Collections.Generic;

namespace Dropbox.Api
{
    public class DropboxFile
    {
        public string Description { get; internal set; }

        public long Bytes { get; internal set; }

        public bool IsDirectory { get; internal set; }

        public string Path { get; internal set; }

        public byte[] Data;

        public IEnumerable<DropboxFile> Contents { get; internal set; }

        public DateTime Modified { get; internal set; }
    }
}
