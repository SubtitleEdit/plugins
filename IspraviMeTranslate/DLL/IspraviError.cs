using System.Collections.Generic;

namespace SubtitleEdit
{
    public class IspraviResult
    {
        public IspraviStatus status { get; set; }

        public IspraviResponse response { get; set; }
    }

    public class IspraviStatus
    {
        public string text { get; set; }
        public int code { get; set; }
    }
    public class IspraviResponse
    {
        /// <summary>
        /// Number of errors
        /// </summary>
        public int errors { get; set; }

        public List<IspraviError> error { get; set; }
    }

    public class IspraviError
    {
        public string suspicious { get; set; }
        public string @class { get; set; }
        public int length { get; set; }
        public int occurrences { get; set; }
        public List<int> position { get; set; }
        public List<string> suggestions { get; set; }
    }
}
