using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;
using System.Collections.Specialized;

namespace Springnote
{
    [XmlRoot(ElementName = "note", Namespace = "http://api.springnote.com")]
    public class Note : ResourceBase
    {
        [XmlElement(ElementName = "identifier")]
        public string Identifier { get; set; }
        
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }

        public Note()
        {
        }

        public Note(string noteName, Consumer consumer)
        {
            this.consumer = consumer;
            this.Identifier = noteName;
        }

        public Page FindPage(string pageId)
        {
            return Page.Find(this, pageId);
        }

        public NameValueCollection GetDefaultParamerers()
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("domain", Identifier);
            return nvc;
        }

        public static List<Note> GetList(Consumer consumer)
        {
            List<Note> ret = new List<Note>();
            string xml = consumer.Get(ResourceBase.BuildUrl("notes"));

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(xml);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr.AddNamespace("sn", "http://api.springnote.com");

            foreach (XmlNode node in xdoc.SelectNodes("/sn:notes/sn:note", nsmgr))
            {
                Note note = (Note)Deserialize(typeof(Note), node.OuterXml);
                note.consumer = consumer;
                ret.Add(note);
            }

            return ret;
        }
    }
}
