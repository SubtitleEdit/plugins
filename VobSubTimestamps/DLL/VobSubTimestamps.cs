using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class VobSubTimestamps : IPlugin
    {
        public string Name => "VobSub Timestamps";

        public string Text => "Replace IDX timestamps";

        public decimal Version => 0.1M;

        public string Description => "Replace the timestamps in the previously imported VobSub IDX file with the time codes from the current subtitle.";

        public string ActionType => "sync";

        public string Shortcut => string.Empty;

        public string DoAction(Form parentForm, string subRipText, double frameRate, string listViewLineSeparatorString, string srtFileName, string videoFileName, string rawText)
        {
            if (string.IsNullOrWhiteSpace(subRipText))
            {
                MessageBox.Show(parentForm, "No subtitle loaded", parentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (string.IsNullOrWhiteSpace(srtFileName))
            {
                MessageBox.Show(parentForm, "No subtitle file name", parentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                var srtRegex = new Regex(@"^([0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3}) +--> +[0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
                var idxRegex = new Regex(@"^timestamp: +([0-9]{2}:[0-9]{2}:[0-9]{2}:[0-9]{3}), +filepos: +[0-9A-Fa-f]{9}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
                var fileName = Path.ChangeExtension(srtFileName, ".idx");
                try
                {
                    var utf8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
                    var idxLines = new List<string>(10000);
                    var srtCodes = new List<string>(9950);
                    int srtIndex = 0;

                    foreach (var line in subRipText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
                    {
                        var match = srtRegex.Match(line);
                        if (match.Success)
                        {
                            srtCodes.Add(match.Groups[1].Value.Replace(',', ':'));
                        }
                    }

                    foreach (var line in File.ReadLines(fileName, utf8Encoding))
                    {
                        var match = idxRegex.Match(line);
                        if (match.Success)
                        {
                            if (srtIndex == srtCodes.Count)
                            {
                                throw new Exception("Too many timestamps in IDX file");
                            }
                            idxLines.Add(line.Remove(match.Groups[1].Index, match.Groups[1].Length).Insert(match.Groups[1].Index, srtCodes[srtIndex++]));
                        }
                        else
                        {
                            idxLines.Add(line);
                        }
                    }
                    if (srtIndex != srtCodes.Count)
                    {
                        throw new Exception("Too many time codes in SubRip subtitle");
                    }
                    {
                        var origFileName = Path.ChangeExtension(fileName, ".orig.idx");
                        if (!File.Exists(origFileName))
                        {
                            File.Copy(fileName, origFileName);
                        }
                        File.WriteAllLines(fileName, idxLines, utf8Encoding);
                    }
                    MessageBox.Show(parentForm, $"{Path.GetFileName(fileName)}\n\n{srtIndex} VobSub timestamps replaced", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(parentForm, $"{Path.GetFileName(fileName)}\n\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return null;
        }

    }
}
