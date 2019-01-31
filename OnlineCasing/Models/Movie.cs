using System;

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
}
