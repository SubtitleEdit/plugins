namespace BaiduTranslate.Translator
{
    public class TranslationPair
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public TranslationPair()
        {
            
        }

        public TranslationPair(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public override string ToString()
        {
            if (Name != null && Name.Length > 1)
                return Name.Substring(0, 1).ToUpperInvariant() + Name.Substring(1).ToLowerInvariant();
            return Name;
        }
    }
}
