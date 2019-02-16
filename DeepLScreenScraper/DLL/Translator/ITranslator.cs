using System.Collections.Generic;
using System.Text;
using WebViewTranslate.Logic;

namespace WebViewTranslate.Translator
{
    public interface ITranslator
    {
        List<TranslationPair> GetTranslationPairs();
        string GetName();
        string GetUrl();
        List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log);
    }
}
