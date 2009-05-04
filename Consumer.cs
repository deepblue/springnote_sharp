using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using OAuth;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Specialized;

namespace Springnote
{
    public class Consumer
    {
        public OAuthToken consumerToken { get; set; }
        public OAuthToken requestToken { get; set; }
        public OAuthToken accessToken { get; set; }
        
        public WebProxy Proxy { get; set; }
        public bool Success { get; set; }

        public Consumer(OAuthToken consumerToken)
        {
            this.consumerToken = consumerToken;
            this.Proxy = null;
        }

        public Consumer(OAuthToken consumerToken, OAuthToken accessToken)
        {
            this.consumerToken = consumerToken;
            this.accessToken = accessToken;
            this.Proxy = null;
        }

        private string GetAuthorizeQuery(string tokenKey, string callback)
        {
            return Resources.OAUTH_AUTHORIZE + "?oauth_token=" + tokenKey;
        }

        public OAuthToken GetRequestToken()
        {
            try
            {
                NameValueCollection param = new NameValueCollection();
                string response = OAuthHttp.Post(Resources.OAUTH_REQUEST_TOKEN, param, this.consumerToken, null, null);
                this.requestToken = new OAuthToken(response);
            }
            catch (Exception ex)
            {
                this.Success = false;
                throw ex;
            }

            return this.requestToken;
        }

        public void Authorize(string callbackUrl)
        {
            if (this.requestToken == null)
                throw new NullReferenceException("You must get a request token before obtaining an authorization URL.");

            System.Diagnostics.Process.Start(GetAuthorizeQuery(this.requestToken.TokenKey, callbackUrl));
        }

        public OAuthToken GetAccessToken()
        {
            try
            {
                NameValueCollection param = new NameValueCollection();
                string response = OAuthHttp.Post(Resources.OAUTH_ACCESS_TOKEN, param, this.consumerToken, this.requestToken, this.Proxy);
                this.accessToken = new OAuthToken(response);
                this.Success = true;
            }
            catch (Exception ex)
            {
                this.Success = false;
                throw ex;
            }
            return this.accessToken;
        }

        public string Get(string url)
        {
            return OAuthHttp.Get(url, consumerToken, accessToken, Proxy);
        }

        public string Post(string url, NameValueCollection parameters)
        {
            return OAuthHttp.Post(url, parameters, consumerToken, accessToken, Proxy);
        }

        public string Post(string url, NameValueCollection parameters, string body, string contentType)
        {
            return OAuthHttp.Post(url, parameters, body, contentType, consumerToken, accessToken, Proxy);
        }

        public string Put(string url, NameValueCollection parameters)
        {
            return OAuthHttp.Put(url, parameters, consumerToken, accessToken, Proxy);
        }

        public string Put(string url, NameValueCollection parameters, string body, string contentType)
        {
            return OAuthHttp.Put(url, parameters, body, contentType, consumerToken, accessToken, Proxy);
        }

        public string Upload(string url, NameValueCollection parameters, string path)
        {
            return OAuthHttp.Upload(url, parameters, path, consumerToken, accessToken, Proxy);
        }
    }
}
