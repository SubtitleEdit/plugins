namespace SubtitleEdit.Translator
{
    public class TranslationPair
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public bool HasFormality { get; set; }

        public TranslationPair()
        {
            
        }

        public TranslationPair(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public TranslationPair(string name, string code, bool hasFormality)
        {
            Name = name;
            Code = code;
            HasFormality = hasFormality;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
