using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliDanmu.Models
{
    public class LoginUrl
    {
        public string url { get; set; }
        public string oauthKey { get; set; }
    }

    public class LoginUrlResponse
    {
        public LoginUrl data { get; set; }
        public int code { get; set; }
        public bool status { get; set; }
        public long ts { get; set; }
    }

    public class LoginInfoResponse
    {
        public bool status {get; set; }

        public LoginInfoData data { get; set; }

        public string message { get; set; }

    }

    public class LoginInfoData
    {
        public string url { get; set; }
        public string refreshToken { get; set; }
    }

    public class LoginInfoCookies
    {
        public string[] setCookie { get; set; }
    }
}
