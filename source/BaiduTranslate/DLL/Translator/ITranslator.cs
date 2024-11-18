using System.Collections.Generic;
using System.Text;
using BaiduTranslate.Logic;

namespace BaiduTranslate.Translator
{
    public interface ITranslator
    {
        List<TranslationPair> GetTranslationPairs();
        string GetName();
        string GetUrl();
        List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log);
    }
}
