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
    [XmlRoot(ElementName = "attachment", Namespace = "http://api.springnote.com")]
	public class Attachment : ResourceBase
	{
        [XmlElement(ElementName = "identifier")]
        public string Identifier { get; set; }

        [XmlElement(ElementName = "title")]
        public string Title { get; set; }

        private Page owner;

        public static Attachment CreateWithPath(string path, Page page, bool modifyPage)
        {
            string url = BuildUrl("pages/" + page.Identifier + "/attachments");

            NameValueCollection nvc = page.note.GetDefaultParamerers();
            if (modifyPage == false)
                nvc.Add("modify_page", "false");
            string resp = page.GetConsumer().Upload(url, nvc, path);
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(resp);

            Attachment at = (Attachment)Deserialize(typeof(Attachment), xml.InnerXml);
            at.SetOwner(page);

            return at;
        }

        private void SetOwner(Page page)
        {
            this.owner = page;
        }
	}
}
