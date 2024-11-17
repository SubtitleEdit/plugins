using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class AviWriter : IPlugin // dll file name must "<classname>.dll" - e.g. "SyncViaOtherSubtitle.dll"
    {
        string IPlugin.Name
        {
            get { return "Avi Writer"; }
        }

        string IPlugin.Text
        {
            get { return "Export to .avi file..."; }
        }

        decimal IPlugin.Version
        {
            get { return 0.5M; }
        }

        string IPlugin.Description
        {
            get { return "Write subtitle to avi file (via FFMPEG)"; }
        }

        string IPlugin.ActionType // Can be one of these: file, tool, sync, translate, spellcheck
        {
            get { return "file"; }
        }

        string IPlugin.Shortcut
        {
            get { return string.Empty; }
        }

        public string GetTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            subtitle = subtitle.Trim();
            if (string.IsNullOrEmpty(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }
            // load subtitle text into object
            var list = new List<string>();
            foreach (var line in subtitle.SplitToLines())
                list.Add(line);
            var sub = new Subtitle();
            var srt = new SubRip();
            srt.LoadSubtitle(sub, list, subtitleFileName);

            // write subtitle to file
            var tempDir = GetTemporaryDirectory();
            File.WriteAllText(Path.Combine(tempDir, "Subtitle.srt"), srt.ToText(sub, "temp"));

            // write program to temp dir
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            foreach (var r in asm.GetManifestResourceNames())
            {
                if (r.EndsWith(".gz", StringComparison.Ordinal))
                    WriteAndUnzipRes(asm, r, tempDir);
            }

            // Start exe file
            var procInfo = new ProcessStartInfo();
            procInfo.FileName = Path.Combine(tempDir, "AviFfmpegWriter.exe");
            procInfo.WorkingDirectory = tempDir;
            var p = Process.Start(procInfo);
            p.WaitForExit();

            // Clean up
            try
            {
                Directory.Delete(tempDir, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            /*
            foreach (var r in asm.GetManifestResourceNames())
            {
                if (r.EndsWith(".gz", StringComparison.Ordinal))
                {
                    try
                    {
                        File.Delete(GetFileNameFromRessourceName(r));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        Process.Start(procInfo.WorkingDirectory);
                    }
                }
            }
            try
            {
                File.Delete(Path.Combine(tempDir, "Subtitle.srt"));
                Directory.Delete(tempDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Process.Start(procInfo.WorkingDirectory);
            }*/
            return string.Empty;
        }

        private string GetFileNameFromRessourceName(string resourceName)
        {
            // AviWriter.DLL.AForge.dll.gz
            var name = resourceName.Substring(resourceName.IndexOf(".DLL.", StringComparison.Ordinal) + 5);
            return name.Substring(0, name.Length - 3);
        }

        private void WriteAndUnzipRes(System.Reflection.Assembly asm, string resourceName, string tempDir)
        {
            using (Stream strm = asm.GetManifestResourceStream(resourceName))
            using (var rdr = new BinaryReader(strm))
            using (var fout = new FileStream(Path.Combine(tempDir, GetFileNameFromRessourceName(resourceName)), FileMode.Create, FileAccess.Write))
            using (var zip = new GZipStream(rdr.BaseStream, CompressionMode.Decompress))
            {
                byte[] data = new byte[4069];
                int bytesRead = 1;
                while (bytesRead > 0)
                {
                    bytesRead = zip.Read(data, 0, data.Length);
                    fout.Write(data, 0, bytesRead);
                }
            }
        }
    }
}