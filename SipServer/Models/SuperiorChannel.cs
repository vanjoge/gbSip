using GB28181.XML;
using SipServer.Cascade;
using SipServer.DBModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SipServer.Models
{
    public class SuperiorChannel : TCatalog
    {
        public SuperiorChannel(TCatalog item, string superiorId, string customChannelId, string groupId)
        {
            CopyFrom(item);

            SuperiorId = superiorId;
            CustomChannelId = customChannelId;
            GroupId = groupId;
        }
        public ulong RowId { get; set; }
        /// <summary>
        /// 上级ID
        /// </summary>
        public string SuperiorId { get; set; }
        /// <summary>
        /// 自定义通道ID
        /// </summary>
        public string CustomChannelId { get; set; }
        /// <summary>
        /// 所属分组ID
        /// </summary>
        public string GroupId { get; set; }

        public string GetChannelId()
        {
            return string.IsNullOrEmpty(CustomChannelId) ? ChannelId : CustomChannelId;
        }
    }
}
