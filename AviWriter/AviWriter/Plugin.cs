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
            get { return 0.4M; }
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
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
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
            foreach (string line in subtitle.Replace(Environment.NewLine, "\n").Split('\n'))
                list.Add(line);
            Subtitle sub = new Subtitle();
            SubRip srt = new SubRip();
            srt.LoadSubtitle(sub, list, subtitleFileName);

            // write subtitle to file
            string tempDir = GetTemporaryDirectory();
            File.WriteAllText(Path.Combine(tempDir, "Subtitle.srt"), srt.ToText(sub, "temp"));

            // write program to temp dir
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            foreach (var r in asm.GetManifestResourceNames())
            {
                if (r.EndsWith(".gz"))
                    WriteAndUnzipRes(asm, r, tempDir);
            }

            // Start exe file
            var info = new ProcessStartInfo();
            info.FileName = Path.Combine(tempDir, "AviFfmpegWriter.exe");
            info.WorkingDirectory = tempDir;
            var p = Process.Start(info);
            p.WaitForExit();

            // Clean up
            foreach (var r in asm.GetManifestResourceNames())
            {
                if (r.EndsWith(".gz"))
                {
                    try { File.Delete(GetFileNameFromRessourceName(r)); }
                    catch { }
                }
            }
            try
            {
                File.Delete(Path.Combine(tempDir, "Subtitle.srt"));
                Directory.Delete(tempDir);
            }
            catch
            {
            }


            return string.Empty;
        }

        string GetFileNameFromRessourceName(string resourceName)
        {
            string fname = resourceName.Substring(resourceName.IndexOf(".DLL.") + 5);
            fname = fname.Substring(0, fname.Length - 3);
            return fname;
        }

        private void WriteAndUnzipRes(System.Reflection.Assembly asm, string resourceName, string tempDir)
        {
            Stream strm = asm.GetManifestResourceStream(resourceName);
            if (strm != null)
            {
                var rdr = new StreamReader(strm);
                var fout = new FileStream(Path.Combine(tempDir, GetFileNameFromRessourceName(resourceName)), FileMode.Create, FileAccess.Write);
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
                fout.Close();
                rdr.Close();
            }
        }
    }
}