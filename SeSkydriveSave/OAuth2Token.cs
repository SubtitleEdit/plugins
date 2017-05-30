using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneDriveSave
{
    public class OAuth2Token
    {
        public string token_type { get; set; }
        public long expires_in { get; set; }
        public string scope { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
    }
}
