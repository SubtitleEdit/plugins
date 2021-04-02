using System.Collections.Generic;
using Nikse.SubtitleEdit.PluginLogic.Strategies;

namespace Nikse.SubtitleEdit.PluginLogic.Commands
{
    public interface ICommand
    {
        void Convert(IList<Paragraph> paragraph, IController controller);
    }
}