using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;
using System.Collections.Specialized;
using System.Web;

namespace Springnote
{
    [XmlRoot(ElementName = "page", Namespace = "http://api.springnote.com")]
    public class Page : ResourceBase
    {
        [XmlElement(ElementName = "identifier")]
        public string Identifier { get; set; }

        [XmlElement(ElementName = "source")]
        public string Source { get; set; }

        [XmlElement(ElementName = "title")]
        public string Title { get; set; }

        [XmlElement(ElementName = "relation_is_part_of")]
        public string ParentIdentifier;

        [XmlElement(ElementName = "date_modified")]
        public DateTime ModifiedTime;

        [XmlElement(ElementName = "date_created")]
        public DateTime CreatedTime;

        [XmlIgnore]
        public Note note { get; set; }
        

        public static Page Find(Note note, string pageId)
        {
            string resp = note.GetConsumer().Get(BuildPageUrl(pageId, note.GetDefaultParamerers()));

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(resp);

            Page page = (Page)Deserialize(typeof(Page), xml.InnerXml);
            page.note = note;
            return page;
        }

        new public Consumer GetConsumer()
        {
            return this.consumer == null ? note.GetConsumer() : this.consumer;
        }

        public Boolean AppendSource(string text, string position)
        {
            NameValueCollection nvc = note.GetDefaultParamerers();
            GetConsumer().Put(BuildPageUrl(), nvc, AppendSourceXml(text, position), "application/xml");

            return true;
        }

        public Attachment CreateAttachment(string path, bool modifyPage)
        {
            return Attachment.CreateWithPath(path, this, modifyPage);
        }

        private string AppendSourceXml(string text, string position)
        {
            StringBuilder output = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            XmlWriter writer = XmlTextWriter.Create(output, settings);
            writer.WriteStartElement("page");

            writer.WriteStartElement("source");
            writer.WriteValue(text);
            writer.WriteEndElement();

            writer.WriteStartElement("source_position");
            writer.WriteValue(position);
            writer.WriteEndElement();
            writer.Close();

            return output.ToString();
        }

        public Boolean Save()
        {
            string xml = Serialize();
            NameValueCollection nvc = note.GetDefaultParamerers();

            if (this.Identifier == String.Empty)
            {
                GetConsumer().Post(BuildPageUrl(), nvc, xml, "application/xml");
            }
            else
            {
                GetConsumer().Put(BuildPageUrl(), nvc, xml, "application/xml");
            }
            

            return true;
        }

        public string BuildPageUrl()
        {
            return BuildPageUrl(new NameValueCollection());
        }

        public string BuildPageUrl(NameValueCollection parameters)
        {
            return Page.BuildPageUrl(this.Identifier, parameters);
        }

        protected static string BuildPageUrl(string pageId)
        {
            return Page.BuildPageUrl(pageId, new NameValueCollection());
        }

        protected static string BuildPageUrl(string pageId, NameValueCollection parameters)
        {
            return BuildUrl("pages/" + pageId, parameters );
        }
    }
}
