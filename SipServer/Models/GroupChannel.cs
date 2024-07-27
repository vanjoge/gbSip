using GB28181.XML;
using SipServer.Cascade;
using SipServer.DBModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SipServer.Models
{
    public class GroupChannel : TCatalog
    {
        public GroupChannel(TCatalog item, TGroupBind bind)
        {
            CopyFrom(item);
            if (bind != null)
            {
                CustomChannelId = bind.CustomChannelId;
                GroupId = bind.GroupId;
            }
        }
        /// <summary>
        /// 自定义通道ID
        /// </summary>
        public string CustomChannelId { get; set; }
        /// <summary>
        /// 所属分组ID
        /// </summary>
        public string GroupId { get; set; }

    }
}
