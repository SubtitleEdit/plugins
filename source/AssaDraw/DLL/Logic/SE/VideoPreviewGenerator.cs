using SubtitleEdit.Logic;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

namespace Nikse.SubtitleEdit.Logic
{
    public class VideoPreviewGenerator
    {
        public static string GetVideoPreviewFileName(int width, int height, Bitmap backgroundImage)
        {
            if (width % 2 != 0)
            {
                width++;
            }

            if (height % 2 != 0)
            {
                height++;
            }

            var backgroundImageHash = string.Empty;
            if (backgroundImage != null)
            {
                backgroundImageHash = $"_{backgroundImage.GetHashCode()}";
            }

            var previewFileName = Path.Combine(Configuration.DataDirectory, $"preview_{width}x{height}{backgroundImageHash}.mkv");
            if (File.Exists(previewFileName))
            {
                return previewFileName;
            }

            try
            {
                var process = GetFFmpegProcess(Color.Black, previewFileName, width, height, 3, 25);
                var tempFileName = string.Empty;
                if (backgroundImage != null)
                {
                    tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
                    backgroundImage.Save(tempFileName, ImageFormat.Jpeg);
                    process = GetFFmpegProcess(tempFileName, previewFileName, width, height, 3, 25);
                }

                process.Start();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(tempFileName))
                {
                    try
                    {
                        File.Delete(tempFileName);
                    }
                    catch
                    {
                        // ignore
                    }
                }

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
            return new Process
            {
                StartInfo =
                {
                    FileName = GetFfmpegLocation(),
                    Arguments = $"-i \"{inputVideoFileName}\" -vf \"ass={Path.GetFileName(assaSubtitleFileName)}\" -strict -2 \"{outputVideoFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(assaSubtitleFileName),
                }
            };
        }

        private static Process GetFFmpegProcess(string imageFileName, string outputFileName, int videoWidth, int videoHeight, int seconds, decimal frameRate)
        {
            return new Process
            {
                StartInfo =
                {
                    FileName = GetFfmpegLocation(),
                    Arguments = $"-t {seconds} -loop 1 -r {frameRate.ToString(CultureInfo.InvariantCulture)} -i \"{imageFileName}\" -c:v libx264 -tune stillimage -shortest -s {videoWidth}x{videoHeight} \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }

        private static Process GetFFmpegProcess(Color color, string outputFileName, int videoWidth, int videoHeight, int seconds, decimal frameRate)
        {
            var htmlColor = $"#{(color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2")).ToUpperInvariant()}";
            return new Process
            {
                StartInfo =
                {
                    FileName = GetFfmpegLocation(),
                    Arguments = $"-t {seconds} -f lavfi -i color=c={htmlColor}:r={frameRate.ToString(CultureInfo.InvariantCulture)}:s={videoWidth}x{videoHeight} -c:v libx264 -tune stillimage -shortest -s {videoWidth}x{videoHeight} \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }

        private static string GetFfmpegLocation()
        {
            var ffmpegLocation = Path.Combine(Configuration.DataDirectory, "ffmpeg\\ffmpeg.exe");
            if (!Configuration.IsRunningOnWindows)
            {
                ffmpegLocation = "ffmpeg";
            }

            return ffmpegLocation;
        }

        /// <summary>
        /// Get screen shot from video at a time code.
        /// </summary>
        /// <param name="inputFileName">Input video file name</param>
        /// <param name="timeCode">time code in format hh:mm:ss[.xxx]</param>
        /// <returns>png file with screen shot</returns>
        public static string GetScreenShot(string inputFileName, string timeCode)
        {
            var outputFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
            var process = new Process
            {
                StartInfo =
                {
                    FileName = GetFfmpegLocation(),
                    Arguments = $"-ss {timeCode} -i \"{inputFileName}\" -frames:v 1 -q:v 2 \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
            return outputFileName;
        }
    }
}
