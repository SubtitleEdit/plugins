using System.Collections.Generic;
using TMDbLib.Objects.Search;

namespace OnlineCasing
{


    public class Settings
    {
        public string ApiKey { get; set; }

        public List<Movie> Movies { get; set; }

        public HashSet<string> IgnoreWords { get; set; }

    }
}
