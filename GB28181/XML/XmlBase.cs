using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace GB28181.XML
{
    public class XmlBase
    {
        public virtual string ToXmlStr()
        {
            var stream = new MemoryStream();
            var xml = new XmlSerializer(this.GetType());
            try
            {
                var xns = new XmlSerializerNamespaces();
                xns.Add("", "");
                //序列化对象
                xml.Serialize(stream, this, xns);
            }
            catch (Exception ex)
            {
                SQ.Base.ErrorLog.WriteLog4Ex("序列化对象为xml字符串出错", ex);
            }
            return System.Text.Encoding.UTF8.GetString(stream.ToArray());//.Replace("\r", "");
        }

    }
}
