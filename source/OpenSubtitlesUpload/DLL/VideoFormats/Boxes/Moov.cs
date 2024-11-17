using System.Collections.Generic;
using System.IO;

namespace OpenSubtitlesUpload.VideoFormats.Boxes
{
    public class Moov : Box
    {
        public Mvhd Mvhd;
        public readonly List<Trak> Tracks;

        public Moov(FileStream fs, ulong maximumLength)
        {
            Tracks = new List<Trak>();
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                    return;

                if (Name == "trak")
                    Tracks.Add(new Trak(fs, Position));
                else if (Name == "mvhd")
                    Mvhd = new Mvhd(fs);

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }
    }
}
