using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleEdit
{
    public class IspraviResult
    {
        public IspraviStatus Status { get; set; }
        public IspraviResponse Response { get; set; }
    }

    public class IspraviStatus
    {
        public string Text { get; set; }
        public int Code { get; set; }
    }
    public class IspraviResponse
    {
        public string Errors { get; set; }
        public List<IspraviError> Error { get; set; }
    }

    public class IspraviError
    {
        public string Suspicious { get; set; }
    }
}
