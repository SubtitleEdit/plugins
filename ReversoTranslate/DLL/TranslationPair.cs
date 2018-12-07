namespace SubtitleEdit
{
    public class TranslationPair
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string IsoCode { get; set; }
        public override string ToString()
        {
            return Name; // will be displayed in combobox
        }
    }
}
