using System.Collections.Generic;
using Nikse.SubtitleEdit.PluginLogic.Models;

namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies
{
    public class ConverterContext
    {
        public IList<Record> Records { get; }

        public ConverterContext()
        {
            Records = new List<Record>();
        }

        public void AddResult(string before, string after, string comment, Paragraph p)
        {
            Records.Add(new Record
            {
                Before = before,
                After = after,
                Comment = comment,
                Paragraph = p
            });
        }
    }
}