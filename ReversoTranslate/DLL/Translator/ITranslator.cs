using System.Collections.Generic;
using System.Text;
using SubtitleEdit.Logic;

namespace SubtitleEdit.Translator
{
    public interface ITranslator
    {
        string GetName();
        string GetUrl();
        List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log);
    }
}
