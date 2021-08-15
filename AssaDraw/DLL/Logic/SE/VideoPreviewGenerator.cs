using SubtitleEdit.Logic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace Nikse.SubtitleEdit.Logic
{
    public class VideoPreviewGenerator
    {
        public static string GetVideoPreviewFileName(int width, int height)
        {
            if (width % 2 != 0)
            {
                width++;
            }

            if (height % 2 != 0)
            {
                height++;
            }

            var previewFileName = Path.Combine(Configuration.DataDirectory, $"preview_{width}x{height}.mkv");
            if (File.Exists(previewFileName))
            {
                return previewFileName;
            }

            try
            {
                var process = GetFFmpegProcess(Color.Black, previewFileName, width, height, 3, 25);
                process.Start();
                process.WaitForExit();

                return previewFileName;
            }
            catch
            {
                previewFileName = Path.Combine(Configuration.DataDirectory, "preview.mkv");
                if (File.Exists(previewFileName))
                {
                    return previewFileName;
                }

                return null;
            }
        }

        /// <summary>
        /// Generate a video with a burned-in Advanced Sub Station Alpha subtitle.
        /// </summary>
        /// <param name="inputVideoFileName">Source video file name</param>
        /// <param name="assaSubtitleFileName">Source subtitle file name</param>
        /// <param name="outputVideoFileName">Output video file name with burned-in subtitle</param>
        public static Process GenerateHardcodedVideoFile(string inputVideoFileName, string assaSubtitleFileName, string outputVideoFileName)
        {
            var ffmpegLocation = Path.Combine(Configuration.DataDirectory, "ffmpeg\\ffmpeg.exe");
            if (!Configuration.IsRunningOnWindows)
            {
                ffmpegLocation = "ffmpeg";
            }

            return new Process
            {
                StartInfo =
                {
                    FileName = ffmpegLocation,
                    Arguments = $"-i \"{inputVideoFileName}\" -vf \"ass={Path.GetFileName(assaSubtitleFileName)}\" -strict -2 \"{outputVideoFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(assaSubtitleFileName),
                }
            };
        }

        private static Process GetFFmpegProcess(string imageFileName, string outputFileName, int videoWidth, int videoHeight, int seconds, decimal frameRate)
        {
            var ffmpegLocation = Path.Combine(Configuration.DataDirectory, "ffmpeg\\ffmpeg.exe");
            if (!Configuration.IsRunningOnWindows)
            {
                ffmpegLocation = "ffmpeg";
            }

            return new Process
            {
                StartInfo =
                {
                    FileName = ffmpegLocation,
                    Arguments = $"-t {seconds} -loop 1 -r {frameRate.ToString(CultureInfo.InvariantCulture)} -i \"{imageFileName}\" -c:v libx264 -tune stillimage -shortest -s {videoWidth}x{videoHeight} \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }

        private static Process GetFFmpegProcess(Color color, string outputFileName, int videoWidth, int videoHeight, int seconds, decimal frameRate)
        {
            var ffmpegLocation = Path.Combine(Configuration.DataDirectory, "ffmpeg\\ffmpeg.exe");
            if (!Configuration.IsRunningOnWindows)
            {
                ffmpegLocation = "ffmpeg";
            }

            var htmlColor = $"#{(color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2")).ToUpperInvariant()}";

            return new Process
            {
                StartInfo =
                {
                    FileName = ffmpegLocation,
                    Arguments = $"-t {seconds} -f lavfi -i color=c={htmlColor}:r={frameRate.ToString(CultureInfo.InvariantCulture)}:s={videoWidth}x{videoHeight} -c:v libx264 -tune stillimage -shortest -s {videoWidth}x{videoHeight} \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }
    }
}
