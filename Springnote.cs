using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Springnote
{
    public static class Resources
    {
        public static string API_ROOT = "https://api.springnote.com/";
        public static string OAUTH_REQUEST_TOKEN = "https://api.springnote.com/oauth/request_token";
        public static string OAUTH_AUTHORIZE = "https://api.springnote.com/oauth/authorize";
        public static string OAUTH_ACCESS_TOKEN = "https://api.springnote.com/oauth/access_token";
    }
}
