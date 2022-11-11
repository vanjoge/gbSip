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
        public SuperiorChannel(TCatalog item, TSuperiorChannel channel)
        {
            ChannelId = item.ChannelId;
            DeviceId = item.DeviceId;
            ParentId = item.ParentId;
            Name = item.Name;
            Manufacturer = item.Manufacturer;
            Model = item.Model;
            Owner = item.Owner;
            CivilCode = item.CivilCode;
            Block = item.Block;
            Address = item.Address;
            Parental = item.Parental;
            BusinessGroupId = item.BusinessGroupId;
            SafetyWay = item.SafetyWay;
            RegisterWay = item.RegisterWay;
            CertNum = item.CertNum;
            Certifiable = item.Certifiable;
            ErrCode = item.ErrCode;
            EndTime = item.EndTime;
            Secrecy = item.Secrecy;
            Ipaddress = item.Ipaddress;
            Port = item.Port;
            Password = item.Password;
            Status = item.Status;
            Longitude = item.Longitude;
            Latitude = item.Latitude;
            RemoteEp = item.RemoteEp;
            Online = item.Online;
            OnlineTime = item.OnlineTime;
            OfflineTime = item.OfflineTime;

            if (channel != null)
            {
                SuperiorId = channel.SuperiorId;
                Enable = channel.Enable;
                CustomChannelId = channel.CustomChannelId;
            }
        }
        public ulong RowId { get; set; }
        /// <summary>
        /// 上级ID
        /// </summary>
        public string SuperiorId { get; set; }
        /// <summary>
        /// 启用
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// 自定义通道ID
        /// </summary>
        public string CustomChannelId { get; set; }

        public string GetChannelId()
        {
            return string.IsNullOrEmpty(CustomChannelId) ? ChannelId : CustomChannelId;
        }
    }
}
