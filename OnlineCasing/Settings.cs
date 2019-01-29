using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMDbLib.Objects.Search;

namespace OnlineCasing
{

    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public DateTime ReleaseDate { get; set; }

        public override string ToString() => $"{ Title ?? OriginalTitle} - {ReleaseDate.Year}";

    }


    public class Settings
    {
        public string ApiKey { get; set; }
        public bool MakeUperCase { get; set; }
        public bool CheckLastLine { get; set; }

        public List<Movie> Movies { get; set; }

        public HashSet<string> IgnoreWords { get; set; }

    }
}
