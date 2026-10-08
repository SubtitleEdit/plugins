using SubtitleEdit.Plugins.Transliterate.Engine;

namespace Transliterate.Tests;

public class TransliteratorTests
{
    private static string To(string id, string text, int system = 0) =>
        TextTransliterator.Convert(text, Languages.Find(id)!, Direction.ToLatin, system);

    private static string From(string id, string text, IReadOnlySet<string>? keep = null) =>
        TextTransliterator.Convert(text, Languages.Find(id)!, Direction.FromLatin, 0, keep);

    [Theory]
    [InlineData("Добро јутро, Љубице!", "Dobro jutro, Ljubice!")]
    [InlineData("ЉУБАВ и Џеп", "LJUBAV i Džep")]
    [InlineData("ШТА ЋЕШ ДА РАДИШ, ЂОРЂЕ?", "ŠTA ĆEŠ DA RADIŠ, ĐORĐE?")]
    [InlineData("Ђорђе већ чека џип.", "Đorđe već čeka džip.")]
    [InlineData("<i>Шта?</i> {\\an8}Ћао", "<i>Šta?</i> {\\an8}Ćao")]
    [InlineData("Види www.пример.рс", "Vidi www.пример.рс")]
    public void SerbianToLatin(string input, string expected) => Assert.Equal(expected, To("sr", input));

    [Theory]
    [InlineData("Dobro jutro, Ljubice!", "Добро јутро, Љубице!")]
    [InlineData("LJUBAV i Džep", "ЉУБАВ и Џеп")]
    [InlineData("injekcija i nadživeti", "инјекција и надживети")]
    [InlineData("Gledam YouTube i Netflix", "Гледам YouTube и Netflix")]
    [InlineData("Poglavlje XIV i III", "Поглавље XIV и III")]
    [InlineData("<i>Šta?</i>", "<i>Шта?</i>")]
    public void SerbianFromLatin(string input, string expected) => Assert.Equal(expected, From("sr", input));

    [Fact]
    public void KeepList() =>
        Assert.Equal("OK, гледај TV", From("sr", "OK, gledaj TV", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ok", "TV" }));

    [Theory]
    [InlineData("Ѓорѓи, ќе дојдам на чај.", 0, "Ǵorǵi, ḱe dojdam na čaj.")]
    [InlineData("Ѓорѓи, ќе дојдам на чај.", 1, "Gjorgji, kje dojdam na chaj.")]
    [InlineData("Ѕвезда и џеб", 1, "Dzvezda i dzheb")]
    public void MacedonianToLatin(string input, int system, string expected) => Assert.Equal(expected, To("mk", input, system));

    [Theory]
    [InlineData("Gjorgji, kje dojdam na chaj.", "Ѓорѓи, ќе дојдам на чај.")]
    [InlineData("Ǵorǵi, ḱe dojdam na čaj.", "Ѓорѓи, ќе дојдам на чај.")]
    public void MacedonianFromLatin(string input, string expected) => Assert.Equal(expected, From("mk", input));

    [Fact]
    public void MontenegrinExtraLetters()
    {
        Assert.Equal("Śutra ćemo", To("cnr", "С́утра ћемо"));
        Assert.Equal("С́утра ћемо", From("cnr", "Śutra ćemo"));
    }

    [Theory]
    [InlineData("Здравствуйте! Это Юлия и Пётр.", 0, "Zdravstvuyte! Eto Yuliya i Pyotr.")]
    [InlineData("Ещё ель, щука, объезд", 0, "Yeshchyo yel, shchuka, obyezd")]
    [InlineData("Щука и Юлия, объём", 1, "Ŝuka i Ûliâ, obʺëm")]
    [InlineData("ЖУРНАЛ", 0, "ZHURNAL")]
    public void Russian(string input, int system, string expected) => Assert.Equal(expected, To("ru", input, system));

    [Theory]
    [InlineData("Привіт, Євгене! Ще їжа є?", "Pryvit, Yevhene! Shche yizha ye?")]
    [InlineData("Згорани, Юрій, Мар’яна", "Zghorany, Yurii, Mariana")]
    [InlineData("Україна", "Ukraina")]
    public void Ukrainian(string input, string expected) => Assert.Equal(expected, To("uk", input));

    [Theory]
    [InlineData("Добры дзень, як справы ў цябе?", "Dobry dzen, yak spravy w tsyabe?")]
    public void Belarusian(string input, string expected) => Assert.Equal(expected, To("be", input));

    [Theory]
    [InlineData("България е красива. Щастие!", "Balgaria e krasiva. Shtastie!")]
    [InlineData("София, Пловдив, Ючбунар", "Sofia, Plovdiv, Yuchbunar")]
    public void Bulgarian(string input, string expected) => Assert.Equal(expected, To("bg", input));

    [Theory]
    [InlineData("Сәлеметсіз бе, Қазақстан!", "Sälemetsız be, Qazaqstan!")]
    [InlineData("ИТ", "İT")]
    public void Kazakh(string input, string expected) => Assert.Equal(expected, To("kk", input));

    [Theory]
    [InlineData("Ўзбекистон, Ғайрат, ер, Тошкент", "Oʻzbekiston, Gʻayrat, yer, Toshkent")]
    [InlineData("Шаҳар ва қишлоқ, эрта", "Shahar va qishloq, erta")]
    public void UzbekToLatin(string input, string expected) => Assert.Equal(expected, To("uz", input));

    [Theory]
    [InlineData("Oʻzbekiston, Gʻayrat, yer, Toshkent", "Ўзбекистон, Ғайрат, ер, Тошкент")]
    [InlineData("O'zbekiston va g‘alaba, erta", "Ўзбекистон ва ғалаба, эрта")]
    public void UzbekFromLatin(string input, string expected) => Assert.Equal(expected, From("uz", input));

    [Theory]
    [InlineData("Καλημέρα! Ευχαριστώ πολύ.", "Kalimera! Efcharisto poly.")]
    [InlineData("Πού είσαι;", "Pou eisai?")]
    [InlineData("Ευρώπη, μπάλα, άγγελος, αύριο", "Evropi, bala, angelos, avrio")]
    [InlineData("ΑΘΗΝΑ και Θεσσαλονίκη", "ATHINA kai Thessaloniki")]
    [InlineData("Ταΰγετος", "Taygetos")]
    public void Greek(string input, string expected) => Assert.Equal(expected, To("el", input));

    [Theory]
    [InlineData("안녕하세요! 한국어를 배워요.", "Annyeonghaseyo! Hangugeoreul baewoyo.")]
    [InlineData("신라 국물 좋고 맛있어요", "Silla gungmul joko masisseoyo")]
    [InlineData("서울 부산", "Seoul busan")]
    public void Korean(string input, string expected) => Assert.Equal(expected, To("ko", input));

    [Fact]
    public void LatinTextIsLeftAloneToLatin() => Assert.Equal("Hello, world", To("ru", "Hello, world"));

    [Theory]
    [InlineData("Ђорђе је ту.", "sr")]
    [InlineData("Ќе дојдам кај Ѓорѓи.", "mk")]
    [InlineData("Это Юлия, ещё раз, ты где?", "ru")]
    [InlineData("Привіт, Євгене! Ще їжа є?", "uk")]
    [InlineData("Як справы ў цябе? Добры дзень.", "be")]
    [InlineData("Сәлеметсіз бе, Қазақстан!", "kk")]
    [InlineData("Ўзбекистонга хуш келибсиз, ҳа.", "uz")]
    [InlineData("Здравейте! България е красива.", "bg")]
    [InlineData("Καλημέρα!", "el")]
    [InlineData("안녕하세요", "ko")]
    public void DetectsToLatin(string text, string id)
    {
        var detected = ScriptDetector.Detect(new[] { text });
        Assert.Equal(id, detected?.Language.Id);
        Assert.Equal(Direction.ToLatin, detected?.Direction);
    }

    [Fact]
    public void DetectsSerbianLatin()
    {
        var detected = ScriptDetector.Detect(new[] { "Đorđe već čeka, šta ćeš?" });
        Assert.Equal("sr", detected?.Language.Id);
        Assert.Equal(Direction.FromLatin, detected?.Direction);
    }
}
