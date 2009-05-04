using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Specialized;

namespace Springnote
{
    public class ResourceBase
    {
        [XmlIgnore]
        public Consumer consumer { get; set; }

        protected static string BuildUrl(string path, NameValueCollection parameters)
        {
            return OAuth.OAuthHttp.EncodeUrl(Resources.API_ROOT + path + ".xml", parameters);
        }

        protected static string BuildUrl(string path)
        {
            return BuildUrl(path, new NameValueCollection());
        }

        protected static object Deserialize(Type type, string xml)
        {

            XmlSerializer serializer = new XmlSerializer(type);
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] bytes = encoding.GetBytes(xml);
            System.IO.MemoryStream stream = new MemoryStream(bytes);
            object obj = (object)serializer.Deserialize(stream);
            stream.Close();

            return obj;
        }

        protected string Serialize()
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType());

            Stream ms = new MemoryStream();
            XmlWriter writer = new XmlTextWriter(ms, Encoding.UTF8);
            serializer.Serialize(writer, this);

            TextReader tr = new StreamReader(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return tr.ReadToEnd();
        }

        public Consumer GetConsumer()
        {
            return consumer;
        }
    }
}
