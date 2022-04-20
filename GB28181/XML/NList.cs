using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GB28181.XML
{
    public class NList<T> : List<T>, IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.Read();
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                var item = (T)serializer.Deserialize(reader);
                this.Add(item);
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            writer.WriteAttributeString("Num", this.Count.ToString());

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            foreach (var item in this)
            {
                serializer.Serialize(writer, item, ns);
            }
        }

        ///// <summary>
        ///// 设备项
        ///// </summary>    


        //[XmlElement("Item")]
        //public List<T> Items
        //{
        //    get { return _items; }
        //}

    }
}
