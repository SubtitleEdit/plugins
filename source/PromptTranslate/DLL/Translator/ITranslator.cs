﻿using System.Collections.Generic;
using System.Text;
using PromptTranslate.Logic;

namespace PromptTranslate.Translator
{
    public interface ITranslator
    {
        List<TranslationPair> GetTranslationPairs();
        string GetName();
        string GetUrl();
        List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log);
    }
}
