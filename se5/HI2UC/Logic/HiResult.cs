namespace SubtitleEdit.Plugins.HI2UC.Logic;

/// <summary>The result for one subtitle text; <see cref="Unbalanced"/> flags mismatched ()/[] that may need a manual look.</summary>
public sealed record HiResult(string After, bool MoodsChanged, bool NarratorChanged, bool Unbalanced)
{
    public string Comment
    {
        get
        {
            var parts = new List<string>();
            if (MoodsChanged)
            {
                parts.Add("Moods");
            }

            if (NarratorChanged)
            {
                parts.Add("Narrator");
            }

            if (Unbalanced)
            {
                parts.Add("Unbalanced ( ) or [ ] - check this line");
            }

            return string.Join("  ·  ", parts);
        }
    }
}
