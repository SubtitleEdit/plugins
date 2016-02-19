namespace Nikse.SubtitleEdit.PluginLogic
{
    public interface IHearingImpaired
    {
        HIStyle Style { get; set; }

        string ChangeMoodsToUppercase(string text);

        string NarratorToUpper(string text);
    }
}