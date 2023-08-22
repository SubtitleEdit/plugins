using System.Collections.Generic;
using Nikse.SubtitleEdit.PluginLogic.Converters.Strategies;

namespace Nikse.SubtitleEdit.PluginLogic.Converters
{
    public interface ICasingConverter
    {
        void Convert(IList<Paragraph> paragraphs, ConverterContext converterContext);
    }
}