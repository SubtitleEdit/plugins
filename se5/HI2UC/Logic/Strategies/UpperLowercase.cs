namespace SubtitleEdit.Plugins.HI2UC.Logic.Strategies;

public class UpperLowercase : IConverterStrategy
{
    public string Name => "Toggle case";
    public string Example => "Foobar → FoObAr";

    public string Execute(string input)
    {
        var buffer = new char[input.Length];
        var len = input.Length;
        var writeTrack = 0;
        var caseState = true;
        for (var i = 0; i < len; i++)
        {
            var ch = input[i];
            // skip tags
            if (TextUtils.IsOpenTag(ch))
            {
                var closingPair = TextUtils.GetClosingPair(ch);
                // look for closing tag
                while (ch != closingPair)
                {
                    buffer[writeTrack++] = ch;
                    if (++i == len)
                    {
                        return input;
                    }

                    ch = input[i];
                }
            }

            if (!char.IsLetter(ch))
            {
                buffer[writeTrack++] = ch;
            }
            else
            {
                buffer[writeTrack++] = caseState ? char.ToUpper(ch) : char.ToLower(ch);
                caseState = !caseState;
            }
        }

        return new string(buffer, 0, writeTrack);
    }

    public override string ToString() => Name;
}
