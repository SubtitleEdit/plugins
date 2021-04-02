using System.Collections.Generic;

namespace Nikse.SubtitleEdit.PluginLogic.Strategies
{
    public class Controller : IController
    {
        public IList<Record> Records { get; }

        public Controller()
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