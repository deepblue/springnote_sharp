using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OAuth;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections; 

namespace OAuth
{
    public static class OAuthHttp
    {
        public static string Get(string url, OAuthToken consumerToken, OAuthToken oauthToken, WebProxy proxy)
        {
            string nonce, timestamp;
            string signature = GetSignature(WebMethod.GET, consumerToken, oauthToken, url, out timestamp, out nonce);
            HttpWebRequest request = CreateWebRequest(url, WebMethod.GET, nonce, timestamp, signature, consumerToken, oauthToken, proxy);
            return GetWebResponse(request);
        }

        public static string Post(string url, NameValueCollection parameters, OAuthToken consumerToken, OAuthToken oauthToken, WebProxy proxy)
        {
            string nonce, timestamp;
            string fullUrl = EncodeUrl(url, parameters);
            string signature = GetSignature(WebMethod.POST, consumerToken, oauthToken, fullUrl, out timestamp, out nonce);
            HttpWebRequest request = CreateWebRequest(url, WebMethod.POST, nonce, timestamp, signature, consumerToken, oauthToken, proxy);
            WritePostData(parameters, request);
            return GetWebResponse(request);
        }

        public static string Post(string url, NameValueCollection parameters, string body, string contentType, OAuthToken consumerToken, OAuthToken oauthToken, WebProxy proxy)
        {
            string nonce, timestamp;
            string fullUrl = EncodeUrl(url, parameters);
            string signature = GetSignature(WebMethod.POST, consumerToken, oauthToken, fullUrl, out timestamp, out nonce);
            HttpWebRequest request = CreateWebRequest(fullUrl, WebMethod.PUT, nonce, timestamp, signature, contentType, consumerToken, oauthToken, proxy);
            WritePostData(body, request.GetRequestStream(), true);
            return GetWebResponse(request);
        }

        public static string Put(string url, NameValueCollection parameters, OAuthToken consumerToken, OAuthToken oauthToken, WebProxy proxy)
        {
            string nonce, timestamp;
            string fullUrl = EncodeUrl(url, parameters);
            string signature = GetSignature(WebMethod.PUT, consumerToken, oauthToken, fullUrl, out timestamp, out nonce);
            HttpWebRequest request = CreateWebRequest(url, WebMethod.PUT, nonce, timestamp, signature, consumerToken, oauthToken, proxy);
            WritePostData(parameters, request);
            return GetWebResponse(request);
        }

        public static string Put(string url, NameValueCollection parameters, string body, string contentType, OAuthToken consumerToken, OAuthToken oauthToken, WebProxy proxy)
        {
            string nonce, timestamp;
            string fullUrl = EncodeUrl(url, parameters);
            string signature = GetSignature(WebMethod.PUT, consumerToken, oauthToken, fullUrl, out timestamp, out nonce);
            HttpWebRequest request = CreateWebRequest(fullUrl, WebMethod.PUT, nonce, timestamp, signature, contentType, consumerToken, oauthToken, proxy);
            WritePostData(body, request.GetRequestStream(), true);
            return GetWebResponse(request);
        }

        public static string Upload(string url, NameValueCollection parameters, string path, OAuthToken consumerToken, OAuthToken oauthToken, WebProxy proxy)
        {
            List<String> files = new List<String>();
            files.Add(path);         

            return UploadAttachments(url, parameters, files, consumerToken, oauthToken, proxy);
        }

        public static string Delete(string url, OAuthToken consumerToken, OAuthToken oauthToken, WebProxy proxy)
        {
            string nonce, timestamp;
            string signature = GetSignature(WebMethod.DELETE, consumerToken, oauthToken, url, out timestamp, out nonce);
            HttpWebRequest request = CreateWebRequest(url, WebMethod.DELETE, nonce, timestamp, signature, consumerToken, oauthToken, proxy);
            return GetWebResponse(request);

        }

        private static string CreateAuthHeader(WebMethod method, string nonce, string timeStamp, string sig, OAuthToken consumerToken, OAuthToken oauthToken)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("OAuth ");
            //sb.Append("realm=\"" + Resources.API_ROOT + "\",");
            sb.Append("oauth_consumer_key=\"" + consumerToken.TokenKey + "\",");

            if (oauthToken != null && oauthToken.TokenKey.Length > 0)
                sb.Append("oauth_token=\"" + oauthToken.TokenKey + "\",");

            string authHeader = "oauth_nonce=\"" + nonce + "\"," +
                                "oauth_timestamp=\"" + timeStamp + "\"," +
                                "oauth_signature_method=\"" + "HMAC-SHA1" + "\"," +
                                "oauth_version=\"" + "1.0" + "\"," +
                                "oauth_signature=\"" +sig + "\"";

            sb.Append(authHeader);
            return sb.ToString();
        }

        public static HttpWebRequest CreateWebRequest(WebMethod method, WebProxy proxy, string requestUrl, bool preAuth)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUrl);
            request.Method = method.ToString();
            request.PreAuthenticate = preAuth;
            request.Proxy = proxy;

            return request;
        }

        private static HttpWebRequest CreateWebRequest(string fullUrl, WebMethod method, string nonce, string timeStamp, string sig, OAuthToken consumerToken, OAuthToken oauthToken, WebProxy proxy)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fullUrl);
            request.Method = method.ToString();
            request.Proxy = proxy;
            string authHeader = CreateAuthHeader(method, nonce, timeStamp, sig, consumerToken, oauthToken);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("Authorization", authHeader);

            return request;
        }

        public static HttpWebRequest CreateWebRequest(string fullUrl, WebMethod method, string nonce, string timeStamp, string sig, string contentType, OAuthToken consumerToken, OAuthToken oauthToken, WebProxy proxy)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fullUrl);
            request.Method = method.ToString();
            request.Proxy = proxy;
            string authHeader = CreateAuthHeader(method, nonce, timeStamp, sig, consumerToken, oauthToken);
            //request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.ContentType = contentType;
            request.Headers.Add("Authorization", authHeader);
            return request;
        }

        public static string EncodeUrl(string url, NameValueCollection parameters)
        {
            string fullUrl = string.Empty;
            int count = 0;
            foreach (string key in parameters.Keys)
            {
                if (count == 0)
                    fullUrl = "?" + key + "=" + Rfc3986.Encode(parameters[key]);
                else
                    fullUrl += "&" + key + "=" + Rfc3986.Encode(parameters[key]);
                count++;
            }
            return url + fullUrl;
        }

        private static string GenerateRandomString(int intLenghtOfString)
        {
            StringBuilder randomString = new StringBuilder();
            Random randomNumber = new Random();
            Char appendedChar;
            for (int i = 0; i <= intLenghtOfString; ++i)
            {
                appendedChar = Convert.ToChar(Convert.ToInt32(26 * randomNumber.NextDouble()) + 65);
                randomString.Append(appendedChar);
            }
            return randomString.ToString();
        }

        public static string GetSignature(WebMethod method, OAuthToken consumerToken, OAuthToken oauthToken, string url, out string timestamp, out string nonce)
        {
            OAuthBase oAuth = new OAuthBase();
            nonce = oAuth.GenerateNonce();
            timestamp = oAuth.GenerateTimeStamp();
            string nurl, nrp;

            string tokenKey = oauthToken == null ? String.Empty : oauthToken.TokenKey;
            string tokenSecret = oauthToken == null ? String.Empty : oauthToken.TokenSecret;

            Uri uri = new Uri(url);
            string sig = oAuth.GenerateSignature(
                uri,
                consumerToken.TokenKey,
                consumerToken.TokenSecret,
                tokenKey,
                tokenSecret,
                method.ToString(),
                timestamp,
                nonce,
                OAuthBase.SignatureTypes.HMACSHA1, out nurl, out nrp);

            return System.Web.HttpUtility.UrlEncode(sig);
        }

        public static string GetWebResponse(HttpWebRequest request)
        {
            WebResponse response = null;
            string data = string.Empty;
            try
            {
                response = request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    data = reader.ReadToEnd();
            }
            catch (WebException ex)
            {
                using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8))
                    data = reader.ReadToEnd();
                //System.Console.WriteLine(data);
                //System.Windows.Forms.MessageBox.Show("Error retrieving web response " + ex.Message);
                throw ex;
            }
            finally
            {
                if (response != null)
                    response.Close();
            }

            return data;
        }

        private static void WritePostData(NameValueCollection parameters, HttpWebRequest request)
        {
            int count = 0;
            string queryString = string.Empty;
            foreach (string key in parameters.Keys)
            {
                if (count == 0)
                    queryString = key + "=" + Rfc3986.Encode(parameters[key]);
                else
                    queryString += "&" + key + "=" + Rfc3986.Encode(parameters[key]);
                count++;
            }

            byte[] postDataBytes = Encoding.ASCII.GetBytes(queryString);
            request.ContentLength = postDataBytes.Length;
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(postDataBytes, 0, postDataBytes.Length);
            reqStream.Close();
        }

        private static void WritePostData(string postData, Stream requestStream, bool close)
        {
            byte[] postDataBytes = System.Text.Encoding.UTF8.GetBytes(postData);
            requestStream.Write(postDataBytes, 0, postDataBytes.Length);
            if (close)
                requestStream.Close();
        }


        private static string UploadAttachments(string url, NameValueCollection parameters, List<string> fileNames, OAuthToken consumerToken, OAuthToken oauthToken, WebProxy proxy)
        {
            string nonce, timestamp;
            string beginBoundary = GenerateRandomString(25);
            string contentBoundary = "--" + beginBoundary;
            string endBoundary = contentBoundary + "--";
            string contentTrailer = "\r\n" + endBoundary;
         
            string signature = OAuthHttp.GetSignature(WebMethod.POST, consumerToken, oauthToken, url, out timestamp, out nonce);
            string contentType = "multipart/form-data; boundary=" + beginBoundary;
            HttpWebRequest request = OAuthHttp.CreateWebRequest(url, WebMethod.POST, nonce, timestamp, signature, contentType, consumerToken, oauthToken, proxy);
            Version protocolVersion = HttpVersion.Version11;
            string method = WebMethod.POST.ToString();
            string contentDisposition = "Content-Disposition: form-data; name=";
            request.Headers.Add("Cache-Control", "no-cache");
            request.KeepAlive = true;
            string postParams = GetPostParameters(parameters, contentBoundary, contentDisposition);

            FileInfo[] fi = new FileInfo[fileNames.Count];
            int i = 0;
            long postDataSize = 0;
            int headerLength = 0;
            List<string> fileHeaders = new List<string>();
            AddFileHeaders(fileNames, contentBoundary, contentDisposition, fi, ref i, ref postDataSize, ref headerLength, fileHeaders);
            request.ContentLength = postParams.Length + headerLength + contentTrailer.Length + postDataSize;
            System.IO.Stream io =  request.GetRequestStream();
            WritePostData(postParams, io, false);
            i = 0;
            foreach (string fileName in fileNames)
            {
                WritePostData(fileHeaders[i], io, false);
                WriteFile(io, fileName);
                i++;
            }
            WritePostData(contentTrailer,io, true);

            string response = GetWebResponse(request);
            io.Close();
            request = null;

            return response;
        }

        private static void AddFileHeaders(List<string> fileNames, string contentBoundary, string contentDisposition, FileInfo[] fi, ref int i, ref long postDataSize, ref int headerLength, List<string> fileHeaders)
        {
            foreach (string s in fileNames)
            {
                string contentType = "application/octet-stream";
                string header = contentBoundary + "\r\n" + contentDisposition + "\"Filedata[]" +
                                    "\"; filename=\"" + Path.GetFileName(s) + "\"\r\n" + "Content-type: " + contentType + "\r\n\r\n";
                fi[i] = new FileInfo(s);
                postDataSize += fi[i].Length;
                headerLength += System.Text.Encoding.UTF8.GetBytes(header).Length;
                fileHeaders.Add(header);
                i++;
            }
        }

        private static string GetPostParameters(NameValueCollection parameters, string contentBoundary, string contentDisposition)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < parameters.Count; i++)
                sb.Append(contentBoundary + "\r\n" + contentDisposition + '"' + parameters.GetKey(i) + "\"\r\n\r\n" + parameters[i].ToString() + "\r\n");


            return sb.ToString();
        }

        public static void WriteFile(System.IO.Stream io, string fileName )
        {
            int bufferSize = 10240;
            FileStream readIn = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            readIn.Seek(0, SeekOrigin.Begin); // move to the start of the file
            byte[] fileData = new byte[bufferSize];
            int bytes;
            while ((bytes = readIn.Read(fileData, 0, bufferSize)) > 0)
            {
                // read the file data and send a chunk at a time
                io.Write(fileData, 0, bytes);
            }
            readIn.Close();
        }
    }
} 
