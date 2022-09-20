using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core
{
    public static class LanguageAutoDetect
    {

        private static int GetCount(string text, params string[] words)
        {
            var options = RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;
            var pattern = "\\b(" + string.Join("|", words) + ")\\b";
            return Regex.Matches(text, pattern, options).Count;
        }

        private static int GetCountContains(string text, params string[] words)
        {
            var count = 0;
            for (var i = 0; i < words.Length; i++)
            {
                var regEx = new Regex(words[i]);
                count += regEx.Matches(text).Count;
            }
            return count;
        }

        private static readonly string[] AutoDetectWordsEnglish =
        {
            "we", "are", "and", "your", "what", "[TW]hat's", "You're", "(any|some|every)thing", "money", "because", "human", "because", "welcome", "really", "something", "confusing", "about", "know", "people", "other", "which", "would",
            "these", "could"
        };

        private static readonly string[] AutoDetectWordsDanish =
        {
            "vi", "han", "og", "jeg", "var", "men", "gider", "bliver", "virkelig", "kommer", "tilbage", "Hej", "længere", "gjorde", "dig", "havde", "[Uu]ndskyld", "arbejder", "vidste", "troede", "stadigvæk", "[Mm]åske", "første", "gik",
            "fortælle", "begyndt", "spørgsmål", "pludselig"
        };

        private static readonly string[] AutoDetectWordsNorwegian =
        {
            "vi", "og", "jeg", "var", "men", "igjen", "Nei", "Hei", "noen", "gjøre", "kanskje", "[Tt]renger", "tenker", "skjer", "møte", "veldig", "takk", "penger", "konsept", "hjelp", "forsvunnet",
            "skutt", "sterkt", "minste", "første", "fortsette", "inneholder", "gikk", "fortelle", "begynt", "spørsmål", "plutselig"
        };

        private static readonly string[] AutoDetectWordsSwedish =
        {
            "vi", "är", "och", "Jag", "inte", "för", "måste", "Öppna", "Förlåt", "nånting", "ingenting", "jävla", "Varför", "[Ss]nälla", "fattar", "själv", "säger", "öppna", "jävligt", "dörren", "göra", "behöver", "tillbaka", "Varför",
            "träffa", "kanske", "säga", "hände", "honom", "hennes", "veckor", "tänker", "själv", "pratar", "mycket", "mamma", "dödade", "Ursäkta", "säger", "senaste", "håller", "förstår", "veckan", "varför", "tycker", "första", "gick",
            "berätta", "börjat", "människor", "frågor", "fråga", "plötsligt", "skämt"
        };

        private static readonly string[] AutoDetectWordsSpanish =
        {
            "qué", "eso", "muy", "estoy?", "ahora", "hay", "tú", "así", "cuando", "cómo", "él", "sólo", "quiero", "gracias", "puedo", "bueno", "soy", "hacer", "fue", "eres", "usted", "tienes", "puede", "[Ss]eñor", "ese", "voy", "quién",
            "creo", "hola", "dónde", "sus", "verdad", "quieres", "mucho", "entonces", "estaba", "tiempo", "esa", "mejor", "hombre", "hace", "dios", "también", "están", "siempre", "hasta", "ahí", "siento",
            "puedes", "Dónde", "está", "estoy", "Vámonos", "vistiendo", "Caliente", "chimenea"
        };

        private static readonly string[] AutoDetectWordsItalian =
        {
            "Buongiorno", "Grazie", "Cosa", "quest[ao]", "quell[ao]", "tutt[io]", "[st]uo", "qualcosa", "ancora", "sono", "bene", "più", "andare", "essere", "venire", "abbiamo", "andiamo", "ragazzi", "signore", "numero", "giorno",
            "propriamente", "sensitivo", "negativo", "davvero", "faccio", "voglio", "vuole", "perché", "allora", "niente", "anche", "stai", "detto", "fatto", "hanno", "molto", "stato", "siamo", "così", "vuoi", "noi", "vero", "loro",
            "fare", "gli", "due"
        };

        private static readonly string[] AutoDetectWordsFrench =
        {
            "pas", "[vn]ous", "ça", "une", "pour", "[mt]oi", "dans", "elle", "tout", "plus", "[bmt]on", "suis", "avec", "oui", "fait", "ils", "être", "faire", "comme", "était", "quoi", "ici", "veux",
            "vouloir", "quelque", "pouvoir", "rien", "dit", "où", "votre", "pourquoi", "sont", "cette", "peux", "alors", "comment", "avez", "très", "même", "merci", "ont", "aussi", "chose", "voir",
            "allez", "tous", "ces", "deux", "avoir", " pouvoir", "même", "œil", "sœur",
        };

        private static readonly string[] AutoDetectWordsPortuguese =
        {
            "[Nn]ão", "[Ee]ntão", "uma", "ele", "bem", "isso", "você", "sim", "meu", "muito", "estou", "ela", "fazer", "tem", "já", "minha", "tudo", "só", "tenho", "agora", "vou", "seu", "quem", "há",
            "lhe", "quero", "nós", "são", "ter", "coisa", "dizer", "eles", "pode", "bom", "mesmo", "mim", "estava", "assim", "estão", "até", "quer", "temos", "acho", "obrigado", "também", "tens",
            "deus", "quê", "ainda", "noite", "conversar", "Desculpe", "verdade"
        };

        private static readonly string[] AutoDetectWordsGerman =
        {
            "und", "auch", "sich", "bin", "hast", "möchte", "müssen", "weiß", "[Vv]ielleicht", "Warum", "jetzt", "Verdammt", "bist", "Darum", "sitzt", "Setz", "Das ist", "Du bist", "nicht", "Scheiße",
            "Nein", "für", "gesagt", "zwei", "richtig"
        };

        private static readonly string[] AutoDetectWordsDutch =
        {
            "van", "een", "[Hh]et", "m(ij|ĳ)", "z(ij|ĳ)n", "hebben", "alleen", "Waarom"
        };

        private static readonly string[] AutoDetectWordsPolish =
        {
            "Czy", "ale", "ty", "siê", "się", "jest", "mnie", "Proszę", "życie", "statku", "życia", "Czyli", "Wszystko", "Wiem", "Przepraszam", "dobrze", "chciałam", "Dziękuję", "Żołnierzyk", "Łowca", "został", "stało", "dolarów",
            "wiadomości", "Dobrze", "będzie", "Dzień", "przyszłość", "Uratowałaś", "Cześć", "Trzeba", "zginąć", "walczyć", "ludzkość", "maszyny", "Jeszcze", "okrążenie", "wyścigu", "porządku", "detektywie",
            "przebieralni", "który"
        };

        private static readonly string[] AutoDetectWordsGreek =
        {
            "μου", "[Εε]ίναι", "αυτό", "Τόμπυ", "καλά", "Ενταξει", "πρεπει", "Λοιπον", "τιποτα", "ξερεις"
        };

        private static readonly string[] AutoDetectWordsRussian =
        {
            "[Ээч]?то", "[Нн]е", "[ТтМмбв]ы", "Да", "[Нн]ет", "Он", "его", "тебя", "как", "меня", "Но", "всё", "мне", "вас", "знаю", "ещё", "за", "нас", "чтобы", "был"
        };

        private static readonly string[] AutoDetectWordsBulgarian =
        {
            "беше", "[Бб]лагодаря", "бързо", "вас", "[Вв]аше", "[Вв]ече", "[Вв]иждам", "време", "[Вв]сичк[ио]", "години", "Да", "[Дд]обре", "дяволите",
            "за", "Защо", "защото", "[Зз]начи", "иска[мнш]", "[Кк]ак", "[Кк]акво", "като", "ко[еий]то", "малко", "много", "[Мм]оже[хш]?", "място",
            "нас", "[Нн]е", "н[еи]що", "н[ия]кой", "[Нн]яма", "преди", "повече", "става", "така", "[Тт]ова", "[Тт]олкова", "[Тт]рябва", "тук",
            "Хайде", "човек"
        };

        private static readonly string[] AutoDetectWordsUkrainian =
        {
            "[Нн]і", "[Пп]ривіт", "[Цц]е", "[Щщ]о", "[Йй]ого", "[Вв]ін", "[Яя]к", "[Гг]аразд", "[Яя]кщо", "[Мм]ені", "[Тт]вій", "[Її]х", "[Вв]ітаю", "[Дд]якую", "вже", "було", "був", "цього",
            "нічого", "немає", "може", "знову", "бо", "щось", "щоб", "цим", "тобі", "хотів", "твоїх", "мої", "мій", "має", "їм", "йому", "дуже"
        };

        private static readonly string[] AutoDetectWordsAlbanian =
        {
            "është", "këtë", "Unë", "mirë", "shumë", "Çfarë", "çfarë", "duhet", "Është", "mbrapa", "Faleminderit", "vërtet", "diçka", "gjithashtu", "gjithe", "eshte", "shume", "vetem", "tënde",
            "çmendur", "vullnetin", "vdekur"
        };

        private static readonly string[] AutoDetectWordsArabic =
        {
            "من", "هل", "لا", "في", "لقد", "ما", "ماذا", "يا", "هذا", "إلى", "على", "أنا", "أنت", "حسناً", "أيها", "كان", "كيف", "يكون", "هذه", "هذان", "الذي", "التي", "الذين", "هناك", "هنالك"
        };

        private static readonly string[] AutoDetectWordsFarsi =
        {
            "این", "برای", "اون", "سیب", "کال", "رو", "خيلي", "آره", "بود", "اون", "نيست", "اينجا", "باشه", "سلام", "ميکني", "داري", "چيزي", "چرا", "خوبه"
        };

        private static readonly string[] AutoDetectWordsHebrew =
        {
            "אתה", "אולי", "הוא", "בסדר", "יודע", "טוב"
        };

        private static readonly string[] AutoDetectWordsVietnamese =
        {
            "không", "[Tt]ôi", "anh", "đó", "ông", "tôi", "phải", "người", "được", "Cậu", "chúng", "chuyện", "muốn", "những", "nhiều"
        };

        private static readonly string[] AutoDetectWordsHungarian =
        {
            "hogy", "lesz", "tudom", "vagy", "mondtam", "még", "vagyok", "csak", "Hát", "felesége", "Csak", "utána", "jött",
            "Miért", "Akkor", "magát", "holnap", "Tudja", "Köszönöm", "élet", "Örvendek", "vissza", "hogy", "tudom", "Rendben",
            "Istenem", "Gyerünk", "értem", "vagyok", "hiszem", "történt", "rendben", "olyan", "őket", "vannak", "mindig", "Kérlek",
            "Gyere", "kicsim", "vagyunk"
        };

        private static readonly string[] AutoDetectWordsTurkish =
        {
            "için", "Tamam", "Hayır", "benim", "daha", "deðil", "önce", "lazým", "çalýþýyor", "Aldırma",
            "burada", "efendim", "şey", "çok", "Çok", "için", "Merhaba", "Evet", "kötü", "musun",
            "güzel", "çünkü", "büyük", "Bebeğim", "olduğunu", "istiyorum", "değilsin", "bilmiyorum",
            "otursana", "Selam", "Tabii","konuda","istiyor","Tetekkürler", "istemiyorum", "Gerçekte"
        };

        private static readonly string[] AutoDetectWordsCroatianAndSerbian =
        {
            "sam", "ali", "nije", "Nije", "samo", "ovo", "kako", "dobro", "Dobro", "sve", "tako", "će", "mogu", "ću", "zašto", "nešto", "za", "misliš", "možeš", "možemo", "ništa", "znaš", "ćemo", "znam"
        };

        private static readonly string[] AutoDetectWordsCroatian =
        {
            "što", "ovdje", "gdje", "kamo", "tko", "prije", "uvijek", "vrijeme", "vidjeti", "netko", "vidio", "nitko", "bok", "lijepo", "oprosti", "htio", "mjesto", "oprostite", "čovjek", "dolje", "čovječe", "dvije", "dijete", "dio",
            "poslije", "događa", "vjerovati", "vjerojatno", "vjerujem", "točno", "razumijem", "vidjela", "cijeli", "svijet", "obitelj", "volio", "sretan", "dovraga", "svijetu", "htjela", "vidjeli", "negdje", "želio", "ponovno",
            "djevojka", "umrijeti", "čovjeka", "mjesta", "djeca", "osjećam", "uopće", "djecu", "naprijed", "obitelji", "doista", "mjestu", "lijepa", "također", "riječ", "tijelo"
        };

        private static readonly string[] AutoDetectWordsSerbian =
        {
            "šta", "ovde", "gde", "ko", "pre", "uvek", "vreme", "videti", "neko", "video", "niko", "ćao", "lepo", "izvini", "hteo", "mesto", "izvinite", "čovek", "dole", "čoveče", "dve", "dete", "deo", "posle", "dešava", "verovati",
            "verovatno", "verujem", "tačno", "razumem", "videla", "ceo", "svet", "porodica", "voleo", "srećan", "dođavola", "svetu", "htela", "videli", "negde", "želeo", "ponovo", "devojka", "umreti", "čoveka", "mesta", "deca", "osećam",
            "uopšte", "decu", "napred", "porodicu", "zaista", "mestu", "lepa", "takođe", "reč", "telo"
        };

        private static readonly string[] AutoDetectWordsSerbianCyrillic =
        {
            "ће", "ћемо", "Хоћу", "сам", "али", "није", "само", "ово", "како", "добро", "све", "тако", "ће", "могу", "ћу", "зашто", "нешто", "за", "шта", "овде", "бити", "чини", "учениче", "побегне", "остати", "Један", "Назад", "Молим"
        };

        private static readonly string[] AutoDetectWordsSerbianCyrillicOnly =
        {
            "дођавола", "Дођи", "пронађемо", "ће", "ћемо", "Хоћу", "чекић", "неће",
        };

        private static readonly string[] AutoDetectWordsIndonesian =
        {
            "yang", "tahu", "bisa", "akan", "tahun", "tapi", "dengan", "untuk", "rumah", "dalam", "sudah", "bertemu"
        };

        private static readonly string[] AutoDetectWordsThai =
        {
            "ผู้กอง", "โรเบิร์ต", "วิตตอเรีย", "เข้าใจมั้ย", "คุณตำรวจ", "ราเชล", "เพื่อน", "เลดดิส", "พระเจ้า", "เท็ดดี้", "หัวหน้า", "แอนดรูว์", "ขอโทษครับ", "ขอบคุณ", "วาร์กัส", "ทุกคน",
            "ไม่เอาน่า", "อิซานะ", "มจริง", "รับทราบ", "พอคะ", "ครับ", "อยาตขาป", "ยินดีทีดรูจักคะ", "ปลอดภัยดีนะ", "ทุกคน", "ตอนที", "ขอบคุณครับ", "ขอทษนะคะ", "ขอทษคะ"
        };

        private static readonly string[] AutoDetectWordsKorean =
        {
            "사루", "거야", "엄마", "그리고", "아니야", "하지만", "말이야", "그들은", "우리가", "엄마가", "괜찮아", "일어나", "잘했어", "뭐라고", "있어요",
            "있어요", "보여줘", "괜찮아요", "목소릴", "기억이", "저주를", "좋아요"
        };

        private static readonly string[] AutoDetectWordsMacedonian =
        {
            "господине", "Нема", "господине", "работа", "вселената", "Може", "треба", "Треба", "слетување", "капсулата", "време", "Френдшип", "Прием", "Добро", "пресметки", "Благодарам", "нешто", "Благодарам", "орбитата", "инженер",
            "Харисон", "Фала", "тоалет", "орбита", "знаеме", "Супервизор", "жени", "Добра", "требаат", "што", "дeкa", "eшe", "кучe", "Руиз", "кучeто", "кучињa", "Бјути", "имa", "многу", "кучињaтa", "AДЗЖ", "Животни", "моЖe", "мaчe",
            "мecто", "имaмe", "мaчињa", "пpвото", "пpaвaт", "нeшто", "колку"
        };

        private static readonly string[] AutoDetectWordsFinnish =
        {
            "että", "kuin", "minä", "mitään", "Mutta", "siitä", "täällä", "poika", "Kiitos", "enää", "vielä", "tässä", "sulkeutuu", "Soitetaan", "Soita", "Onnistui", "Mitä", "Etuovi", "tippiä", "antaa", "Onko", "Hidasta", "tuntia",
            "tilata", "päästä", "palolaitokselle", "hätänumeroon", "aikaa", "Tämä", "Sinulla", "Palauttaa", "Kiitos", "Arvostele", "Älä", "toimi", "televisiota", "takaisin", "reitin", "pitäisi", "palauttaa", "nopeamman", "mitään",
            "meidät", "maksaa", "kuullut", "kaikki", "jälkeen", "ihmiset", "hätäkeskukseen", "hiljaa", "haluat", "enää", "enemmän", "auttaa", "Tunkeilijahälytys", "Pysähdy", "Princen", "Käänny", "Kyllä", "Kiva", "Haluatko", "Haluan"
        };

        private static readonly string[] AutoDetectWordsRomanian =
        {
            "pentru", "oamenii", "decât", "[Vv]reau", "[Ss]înt", "Asteaptã", "Fãrã", "aici", "domnule", "trãiascã", "niciodatã", "înseamnã", "vorbesti", "fãcut", "spune", "făcut", "când", "aici", "Asta", "văzut", "dacă", "câteva",
            "amândoi", "Când", "totuși", "mașină", "aceeași", "întâmplat", "niște", "ziua", "noastră", "cunoscut", "decat", "[Tt]rebuie", "[Aa]cum", "Poate", "vrea", "soare", "nevoie", "daca", "echilibrul", "vorbesti", "zeului",
            "atunci", "memoria", "soarele"
        };

        // Czech and Slovak languages have many common words (especially when non flexed)
        private static readonly string[] AutoDetectWordsCzechAndSlovak =
        {
            "[Oo]n[ao]?", "[Jj]?si", "[Aa]le", "[Tt]en(to)?", "[Rr]ok", "[Tt]ak", "[Aa]by", "[Tt]am", "[Jj]ed(en|na|no)", "[Nn]ež", "[Aa]ni", "[Bb]ez", "[Dd]obr[ýáé]", "[Vv]šak", "[Cc]el[ýáé]", "[Nn]ov[ýáé]", "[Dd]ruh[ýáé]", "jsem",
            "poøádku", "Pojïme", "háje", "není", "Jdeme", "všecko", "jsme", "Prosím", "Vezmi", "když", "Takže", "Dìkuji", "prechádzku", "všetko", "Poïme", "potom", "Takže", "Neviem", "budúcnosti", "trochu"
        };
        // differences between Czech and Slovak languages / Czech words / please keep the words aligned between these languages for better comparison
        private static readonly string[] AutoDetectWordsCzech =
        {
            ".*[Řř].*", ".*[ůě].*", "[Bb]ýt", "[Jj]sem", "[Jj]si", "[Jj]á", "[Mm]ít", "[Aa]no", "[Nn]e",  "[Nn]ic", "[Dd]en", "[Jj]en", "[Cc]o", "[Jj]ak[o]?", "[Nn]ebo",  "[Pp]ři", "[Pp]ro", "[Pp]řed.*", "[Jj](ít|du|de|deme|dou)",
            "[Mm]ezi",  "[Jj]eště", "[Čč]lověk", "[Pp]odle", "[Dd]alší"
        };
        // differences between Czech and Slovak languages / Slovak words / please keep the words aligned between these languages for better comparison
        private static readonly string[] AutoDetectWordsSlovak =
        {
            ".*[Ôô].*", ".*[ä].*",  "[Bb]yť", "[Ss]om",  "[Ss]i",  "[Jj]a", "[Mm]ať", "[Áá]no", "[Nn]ie", "[Nn]ič", "[Dd]eň", "[Ll]en", "[Čč]o", "[Aa]ko", "[Aa]?[Ll]ebo", "[Pp]ri", "[Pp]re", "[Pp]red.*", "([Íí]sť|[Ii](dem|de|deme|dú))",
            "[Mm]edzi", "[Ee]šte",  "[Čč]lovek", "[Pp]odľa", "[Ďď]alš(í|ia|ie)"
        };

        private static readonly string[] AutoDetectWordsSlovenian =
        {
            "sem", "bom", "Zakaj", "lahko", "Kje", "Lahko", "Hvala", "Ampak", "nekaj", "Prosim", "prišla", "nisem", "denar", "ampak",
            "všeč", "videti", "stanovanje", "službo", "prosim", "naredila", "moram", "domov", "Vrzi", "Povej", "številko", "zaporu",
            "ugrabila", "ubila", "tvoja", "tudi", "tečna", "stvari", "rusko", "povedala", "obraz", "nalogo", "mislim", "govoriš",
            "Seveda", "Razmišljam", "Potem", "Nič", "Nisem", "Mogoče", "žensko", "žalostna", "človek", "Čisto", "znaš",
            "zlomi", "zgodilo", "zdaj", "zajtrk", "utrujena","ustrelila", "srečna", "čarovnik", "zaživeti",
            "Torej", "plasti","kanjon", "več", "prišlo", "presenečeni", "največje"
        };

        private static readonly string[] AutoDetectWordsIcelandic =
        {
            "við",
            "Það",
            "með",
            "Hvað",
            "fyrir",
            "hvað",
            "verður",
            "segja",
            "þessu",
            "hérna",
            "maður",
            "Mér",
            "Guð",
            "þið",
            "svona",
            "okkur",
            "þau",
            "sé",
            "núna",
            "hefur",
            "hafa",
            "eða",
            "þegar",
            "þarna",
            "verð",
            "veist",
            "ætla",
            "okkar",
            "barnið",
            "eitthvað",
            "ólétt",
            "væri",
            "sagði",
            "Því",
            "ykkur",
            "annað",
            "Frábært",
            "Þakka",
            "góður",
            "Gerðu",
            "Fyrirgefðu",
            "verða",
            "kvöld",
            "alvöru",
            "Veistu",
            "Kannski",
            "Einmitt",
            "verðum",
            "líður",
            "höfum",
            "hugmynd",
            "Heyrðu",
            "skrýtið",
            "stefnumót",
            "skeið",
            "síðan",
            "hingað",
            "ótrúlegt",
            "Þekkirðu",
            "spýtustrákur",
            "miðskóla",
        };

        private static readonly string[] AutoDetectWordsLatvian =
        {
            "Paldies", "neesmu ", "nezinu", "viòð", "viņš", "viņu", "kungs", "esmu", "Viņš", "Velns", "viņa", "dievs", "Pagaidi", "varonis", "agrāk", "varbūt"
        };

        private static readonly string[] AutoDetectWordsLithuanian =
        {
            "tavęs", "veidai", "apie", "jums", "Veidai", "Kaip", "kaip", "reikia", "Šūdas", "frensis", "Ačiū", "vilsonai", "Palauk", "Veidas", "viskas", "Tikrai", "manęs", "Tačiau", "žmogau", "Flagai", "Prašau", "Džiune", "Nakties",
            "šviesybe", "Supratau", "komanda", "reikia", "apie", "Kodėl", "mūsų", "Ačiū", "vyksta"
        };

        private static readonly string[] AutoDetectWordsHindi =
        {
            "एक", "और", "को", "का", "यह", "सकते", "लिए", "करने", "भारतीय", "सकता", "भारत", "तकनीक", "कंप्यूटिंग", "उपकरण", "भाषाओं", "भाषा", "कंप्यूटर", "आप", "आपको", "अपने", "लेकिन", "करना", "सकता", "बहुत", "चाहते", "अच्छा", "वास्तव",
            "लगता", "इसलिए", "शेल्डन", "धन्यवाद।", "तरह", "करता", "चाहता", "कोशिश", "करते", "किया", "अजीब", "सिर्फ", "शुरू"
        };

        private static readonly string[] AutoDetectWordsUrdu =
        {
            "نہیں", "ایک", "ہیں", "کیا", "اور", "لئے", "ٹھیک", "ہوں", "مجھے", "تھا", "کرنے", "صرف", "ارے", "یہاں", "بہت", "لیکن", "ساتھ", "اپنے", "اچھا", "میرے", "چاہتا", "انہوں", "تمہیں"
        };

        private static readonly string[] AutoDetectWordsSinhalese =
        {
            "කරන්න", "නැහැ", "කියල", "මගේ", "එයා", "ඔයාට", "කැප්ටන්", "ඔයාගේ", "පුළුවන්", "හැම", "වගේ", "වෙන්න", "ඕනා", "නෙමේ", "තියෙන්නේ", "වගේම", "දන්නවා", "වෙනවා", "එහෙම", "හිතන්නේ", "කියලා", "කරනවා", "යන්න", "දෙයක්", "තියනවා",
            "හොදයි", "දෙන්න", "එකක්", "මොනාද", "කියන්න", "කරන්නේ", "වෙන්නේ", "රොජර්ස්", "දාන්න", "කරලා", "ඔයාව", "වෙලා", "කොහොමද", "කිවුවා", "ඔබට", "රාවුල්"
        };

        public static string AutoDetectGoogleLanguage(string text, int bestCount)
        {
            var count = GetCount(text, AutoDetectWordsEnglish);
            if (count > bestCount)
            {
                var dutchCount = GetCount(text, AutoDetectWordsDutch);
                if (dutchCount < count)
                {
                    return "en";
                }
            }

            count = GetCount(text, AutoDetectWordsDanish);
            if (count > bestCount)
            {
                var norwegianCount = GetCount(text, "ut", "deg", "meg", "merkelig", "mye", "spørre");
                var dutchCount = GetCount(text, AutoDetectWordsDutch);
                var swedishCount = GetCount(text, AutoDetectWordsSwedish);
                var icelandicCount = GetCount(text, AutoDetectWordsIcelandic);
                if (norwegianCount < 2 && dutchCount < count && swedishCount < count)
                {
                    if (icelandicCount > count * 1.5)
                    {
                        return "is";
                    }

                    return "da";
                }
            }

            count = GetCount(text, AutoDetectWordsNorwegian);
            if (count > bestCount)
            {
                var danishCount = GetCount(text, "siger", "dig", "mig", "mærkelig", "tilbage", "spørge");
                var dutchCount = GetCount(text, AutoDetectWordsDutch);
                var swedishCount = GetCount(text, AutoDetectWordsSwedish);
                if (danishCount < 2 && dutchCount < count && swedishCount < count)
                {
                    return "no";
                }
            }

            count = GetCount(text, AutoDetectWordsSwedish);
            if (count > bestCount)
            {
                return "sv";
            }

            count = GetCount(text, AutoDetectWordsSpanish);
            if (count > bestCount)
            {
                var frenchCount = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not spanish words
                var portugueseCount = GetCount(text, "[NnCc]ão", "Então", "h?ouve", "pessoal", "rapariga", "tivesse", "fizeste",
                                                     "jantar", "conheço", "atenção", "foste", "milhões", "devias", "ganhar", "raios"); // not spanish words
                if (frenchCount < 2 && portugueseCount < 2)
                {
                    return "es";
                }
            }

            count = GetCount(text, AutoDetectWordsItalian);
            if (count > bestCount)
            {
                var frenchCount = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not italian words
                if (frenchCount < 2)
                {
                    return "it";
                }
            }

            count = GetCount(text, AutoDetectWordsFrench);
            if (count > bestCount)
            {
                var romanianCount = GetCount(text, "[Vv]reau", "[Ss]înt", "[Aa]cum", "pentru", "domnule", "aici");
                if (romanianCount < 5)
                {
                    return "fr";
                }
            }

            count = GetCount(text, AutoDetectWordsPortuguese);
            if (count > bestCount)
            {
                var slovenianCount = GetCount(text, AutoDetectWordsSlovenian);
                if (slovenianCount > count)
                {
                    return "sl";
                }

                return "pt"; // Portuguese
            }

            count = GetCount(text, AutoDetectWordsGerman);
            if (count > bestCount)
            {
                return "de";
            }

            count = GetCount(text, AutoDetectWordsDutch);
            if (count > bestCount)
            {
                return "nl";
            }

            count = GetCount(text, AutoDetectWordsPolish);
            if (count > bestCount)
            {
                return "pl";
            }

            count = GetCount(text, AutoDetectWordsGreek);
            if (count > bestCount)
            {
                return "el"; // Greek
            }

            count = GetCount(text, AutoDetectWordsRussian);
            if (count > bestCount)
            {
                var bulgarianCount = GetCount(text, AutoDetectWordsBulgarian);
                if (bulgarianCount > count)
                {
                    return "bg"; // Bulgarian
                }

                var ukrainianCount = GetCount(text, AutoDetectWordsUkrainian);
                if (ukrainianCount > count)
                {
                    return "uk"; // Ukrainian
                }

                var serbianCount = GetCount(text, AutoDetectWordsSerbianCyrillic);
                if (serbianCount > count)
                {
                    return "sr"; // Serbian
                }

                var serbianWordsOnlyCount = GetCount(text, AutoDetectWordsSerbianCyrillicOnly);
                if (serbianWordsOnlyCount > 1)
                {
                    return "sr"; // Serbian
                }

                return "ru"; // Russian
            }

            count = GetCount(text, AutoDetectWordsUkrainian);
            if (count > bestCount)
            {
                return "uk"; // Ukrainian
            }

            count = GetCount(text, AutoDetectWordsBulgarian);
            if (count > bestCount)
            {
                return "bg"; // Bulgarian
            }

            count = GetCount(text, AutoDetectWordsAlbanian);
            if (count > bestCount)
            {
                return "sq"; // Albanian
            }

            count = GetCount(text, AutoDetectWordsArabic);
            if (count > bestCount)
            {
                var hebrewCount = GetCount(text, AutoDetectWordsHebrew);
                var farsiCount = GetCount(text, AutoDetectWordsFarsi);
                if (hebrewCount < count && farsiCount < count)
                {
                    return "ar"; // Arabic
                }
            }

            count = GetCount(text, AutoDetectWordsHebrew);
            if (count > bestCount)
            {
                return "he"; // Hebrew
            }

            count = GetCount(text, AutoDetectWordsFarsi);
            if (count > bestCount)
            {
                return "fa"; // Farsi (Persian)
            }

            count = GetCount(text, AutoDetectWordsCroatianAndSerbian);
            if (count > bestCount)
            {
                var croatianCount = GetCount(text, AutoDetectWordsCroatian);
                var serbianCount = GetCount(text, AutoDetectWordsSerbian);
                var slovenianCount = GetCount(text, AutoDetectWordsSlovenian);
                if (croatianCount > serbianCount)
                {
                    if (slovenianCount > croatianCount)
                    {
                        return "sl";
                    }

                    return "hr"; // Croatian
                }

                if (slovenianCount > count)
                {
                    return "sl";
                }

                return "sr"; // Serbian
            }

            count = GetCount(text, AutoDetectWordsVietnamese);
            if (count > bestCount)
            {
                return "vi"; // Vietnamese
            }

            count = GetCount(text, AutoDetectWordsHungarian);
            if (count > bestCount)
            {
                return "hu"; // Hungarian
            }

            count = GetCount(text, AutoDetectWordsTurkish);
            if (count > bestCount)
            {
                return "tr"; // Turkish
            }

            count = GetCount(text, AutoDetectWordsIndonesian);
            if (count > bestCount)
            {
                return "id"; // Indonesian
            }

            count = GetCount(text, AutoDetectWordsThai);
            if (count > 10 || count > bestCount)
            {
                return "th"; // Thai
            }

            count = GetCount(text, AutoDetectWordsKorean);
            if (count > 10 || count > bestCount)
            {
                return "ko"; // Korean
            }

            count = GetCount(text, AutoDetectWordsFinnish);
            if (count > bestCount)
            {
                return "fi"; // Finnish
            }

            count = GetCount(text, AutoDetectWordsRomanian);
            if (count > bestCount)
            {
                return "ro"; // Romanian
            }

            count = GetCountContains(text, "シ", "ュ", "シン", "シ", "ン", "ユ");
            count += GetCountContains(text, "イ", "ン", "チ", "ェ", "ク", "ハ");
            count += GetCountContains(text, "シ", "ュ", "う", "シ", "ン", "サ");
            count += GetCountContains(text, "シ", "ュ", "シ", "ン", "だ", "う");
            if (count > bestCount * 2)
            {
                return "ja"; // Japanese - not tested...
            }

            count = GetCountContains(text, "是", "是早", "吧", "的", "爱", "上好");
            count += GetCountContains(text, "的", "啊", "好", "好", "亲", "的");
            count += GetCountContains(text, "谢", "走", "吧", "晚", "上", "好");
            count += GetCountContains(text, "来", "卡", "拉", "吐", "滚", "他");
            if (count > bestCount * 2)
            {
                return "zh"; // Chinese (simplified) - not tested...
            }

            count = GetCount(text, AutoDetectWordsCzechAndSlovak);
            if (count > bestCount)
            {
                var lithuanianCount = GetCount(text, AutoDetectWordsLithuanian);
                int finnishCount = GetCount(text, AutoDetectWordsFinnish);
                if (lithuanianCount <= count && finnishCount < count)
                {
                    int czechWordsCount = GetCount(text, AutoDetectWordsCzech);
                    int slovakWordsCount = GetCount(text, AutoDetectWordsSlovak);
                    if (czechWordsCount >= slovakWordsCount)
                    {
                        return "cs"; // Czech
                    }

                    return "sk"; // Slovak
                }
            }

            count = GetCount(text, AutoDetectWordsSlovenian);
            if (count > bestCount)
            {
                return "sl";
            }

            count = GetCount(text, AutoDetectWordsLatvian);
            if (count > bestCount * 1.2)
            {
                return "lv";
            }

            count = GetCount(text, AutoDetectWordsLithuanian);
            if (count > bestCount)
            {
                return "lt";
            }

            count = GetCount(text, AutoDetectWordsHindi);
            if (count > bestCount)
            {
                return "hi";
            }

            count = GetCount(text, AutoDetectWordsUrdu);
            if (count > bestCount)
            {
                return "ur";
            }

            count = GetCount(text, AutoDetectWordsSinhalese);
            if (count > bestCount)
            {
                return "si";
            }

            count = GetCount(text, AutoDetectWordsMacedonian);
            if (count > bestCount)
            {
                return "mk";
            }

            count = GetCount(text, AutoDetectWordsIcelandic);
            if (count > bestCount)
            {
                return "is";
            }

            return string.Empty;
        }

    }
}
