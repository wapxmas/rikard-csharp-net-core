using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace RikardWeb.Lib.Adverts.Data
{
    [Serializable]
    [XmlRoot(ElementName = "notification", Namespace = "http://torgi.gov.ru/opendata")]
    public class NotificationInfo
    {
        [XmlElement("bidKindId")]
        public int BidKindId { get; set; }

        [XmlElement("bidKindName")]
        public string BidKindName { get; set; }

        [XmlElement("bidNumber")]
        public string BidNumber { get; set; }

        [XmlElement("organizationName")]
        public string OrganizationName { get; set; }

        [XmlElement("isArchived")]
        public int IsArchived { get; set; }

        [XmlElement("publishDate")]
        public string PublishDate { get; set; }

        [XmlElement("lastChanged")]
        public string LastChanged { get; set; }

        [XmlElement("odDetailedHref")]
        public string OdDetailedHref { get; set; }
    }
}
