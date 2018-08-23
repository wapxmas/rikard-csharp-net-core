using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RikardLib.Text
{
    public static class XmlUtilites
    {
        public static T DeserializeXmlString<T>(string xml)
        {
            StringReader sr = new StringReader(xml);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            return (T)xmlSerializer.Deserialize(sr);
        }

        public static T ReadXmlOnce<T>(string filename, string elementName, bool debug = false)
        {
            return ReadXmlStream<T>(filename, elementName, debug).FirstOrDefault();
        }

        public static IEnumerable<T> ReadXmlStream<T>(string filename, string elementName, bool debug = false)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == elementName)
                        {
                            var outerXml = reader.ReadOuterXml();

                            if(debug)
                            {
                                Console.WriteLine(outerXml);
                            }

                            StringReader sr = new StringReader(outerXml);

                            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

                            yield return (T)xmlSerializer.Deserialize(sr);
                        }
                    }
                }
            }
        }
    }
}
