﻿using System;
using System.Collections.Generic;
using System.IO;
using OpenSubtitlesUpload.VideoFormats.Boxes;

namespace OpenSubtitlesUpload.VideoFormats
{
    /// <summary>
    /// http://wiki.multimedia.cx/index.php?title=QuickTime_container
    /// </summary>
    internal class MP4 : Box
    {
        public string FileName { get; private set; }
        public Moov Moov { get; private set; }

        public List<Trak> GetSubtitleTracks()
        {
            var list = new List<Trak>();
            if (Moov != null && Moov.Tracks != null)
            {
                foreach (var trak in Moov.Tracks)
                {
                    if (trak.Mdia != null && (trak.Mdia.IsTextSubtitle || trak.Mdia.IsVobSubSubtitle || trak.Mdia.IsClosedCaption) && trak.Mdia.Minf != null && trak.Mdia.Minf.Stbl != null)
                    {
                        list.Add(trak);
                    }
                }
            }
            return list;
        }

        public List<Trak> GetAudioTracks()
        {
            var list = new List<Trak>();
            if (Moov != null && Moov.Tracks != null)
            {
                foreach (var trak in Moov.Tracks)
                {
                    if (trak.Mdia != null && trak.Mdia.IsAudio)
                    {
                        list.Add(trak);
                    }
                }
            }
            return list;
        }

        public List<Trak> GetVideoTracks()
        {
            var list = new List<Trak>();
            if (Moov != null && Moov.Tracks != null)
            {
                foreach (var trak in Moov.Tracks)
                {
                    if (trak.Mdia != null && trak.Mdia.IsVideo)
                    {
                        list.Add(trak);
                    }
                }
            }
            return list;
        }

        public TimeSpan Duration
        {
            get
            {
                if (Moov != null && Moov.Mvhd != null && Moov.Mvhd.TimeScale > 0)
                    return TimeSpan.FromSeconds((double)Moov.Mvhd.Duration / Moov.Mvhd.TimeScale);
                return new TimeSpan();
            }
        }

        public DateTime CreationDate
        {
            get
            {
                if (Moov != null && Moov.Mvhd != null && Moov.Mvhd.TimeScale > 0)
                    return new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(TimeSpan.FromSeconds(Moov.Mvhd.CreationTime));
                return DateTime.Now;
            }
        }

        /// <summary>
        /// Resolution of first video track. If not present returns 0.0
        /// </summary>
        public System.Drawing.Point VideoResolution
        {
            get
            {
                if (Moov != null && Moov.Tracks != null)
                {
                    foreach (var trak in Moov.Tracks)
                    {
                        if (trak != null && trak.Mdia != null && trak.Tkhd != null)
                        {
                            if (trak.Mdia.IsVideo)
                                return new System.Drawing.Point((int)trak.Tkhd.Width, (int)trak.Tkhd.Height);
                        }
                    }
                }
                return new System.Drawing.Point(0, 0);
            }
        }

        public MP4(string fileName)
        {
            FileName = fileName;
            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                ParseMp4(fs);
        }

        public MP4(FileStream fs)
        {
            FileName = null;
            ParseMp4(fs);
        }

        private void ParseMp4(FileStream fs)
        {
            int count = 0;
            Position = 0;
            fs.Seek(0, SeekOrigin.Begin);
            using (fs)
            {
                bool moreBytes = true;
                while (moreBytes)
                {
                    moreBytes = InitializeSizeAndName(fs);
                    if (Size < 8)
                        return;

                    if (Name == "moov" && Moov == null)
                        Moov = new Moov(fs, Position);

                    count++;
                    if (count > 100)
                        break;

                    if (Position > (ulong)fs.Length)
                        break;
                    fs.Seek((long)Position, SeekOrigin.Begin);
                }
            }
        }

        public double FrameRate
        {
            get
            {
                // Formula: moov.mdia.stbl.stsz.samplecount / (moov.trak.tkhd.duration / moov.mvhd.timescale)
                if (Moov != null && Moov.Mvhd != null && Moov.Mvhd.TimeScale > 0)
                {
                    var videoTracks = GetVideoTracks();
                    if (videoTracks.Count > 0 && videoTracks[0].Tkhd != null && videoTracks[0].Mdia != null && videoTracks[0].Mdia.Minf != null && videoTracks[0].Mdia.Minf.Stbl != null)
                    {
                        double duration = videoTracks[0].Tkhd.Duration;
                        double sampleCount = videoTracks[0].Mdia.Minf.Stbl.StszSampleCount;
                        return sampleCount / (duration / Moov.Mvhd.TimeScale);
                    }
                }
                return 0;
            }

        }

    }
}
