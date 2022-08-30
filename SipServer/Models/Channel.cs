using System;
using System.Collections.Generic;
using GB28181.XML;
using SipServer.DBModel;
using SQ.Base;

namespace SipServer.Models
{
    public partial class Channel : TCatalog
    {
        public Channel()
        {

        }
        public Channel(TCatalog u)
        {
            Address = u.Address;
            Block = u.Block;
            BusinessGroupId = u.BusinessGroupId;
            Certifiable = u.Certifiable;
            CertNum = u.CertNum;
            CivilCode = u.CivilCode;
            ChannelId = u.ChannelId;
            EndTime = u.EndTime;
            ErrCode = u.ErrCode;
            DeviceId = u.DeviceId;
            Ipaddress = u.Ipaddress;
            Latitude = u.Latitude;
            Longitude = u.Longitude;
            Manufacturer = u.Manufacturer;
            Model = u.Model;
            Name = u.Name;
            Owner = u.Owner;
            Parental = u.Parental;
            ParentId = u.ParentId;
            Password = u.Password;
            Port = u.Port;
            RegisterWay = u.RegisterWay;
            SafetyWay = u.SafetyWay;
            Secrecy = u.Secrecy;
            Status = u.Status;
            Online = u.Online;
        }

        ///// <summary>
        ///// 远程设备终结点
        ///// </summary>
        //public string RemoteEp { get; set; }
        /// <summary>
        /// 别名
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 网络传输模式(0 自动; 1 TCP被动; 2 TCP主动; 3 UDP)
        /// </summary>
        public int NetType { get; set; }
        /// <summary>
        /// 对讲模式(0 自动; 1 Invite被动; 2 Invite主动; 3 广播)
        /// </summary>
        public int TalkType { get; set; }

        public void SetChannelConf(ChannelConf conf)
        {
            this.RemoteEp = conf.RemoteEp;
            this.NickName = conf.NickName;
            this.NetType = conf.NetType;
            this.TalkType = conf.TalkType;
        }
        public void FillChannelConf(ChannelConf conf)
        {
            conf.RemoteEp = this.RemoteEp;
            conf.NickName = this.NickName;
            conf.NetType = this.NetType;
            conf.TalkType = this.TalkType;
        }
        public ChannelConf ToChannelConf()
        {
            var item = new ChannelConf();
            FillChannelConf(item);
            return item;
        }
    }
}
