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
            CopyFrom(u);
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
        /// <summary>
        /// RTVS视频服务地址
        /// </summary>
        public string RTVSVideoServer { get; set; }
        /// <summary>
        /// RTVS视频服务端口
        /// </summary>
        public int RTVSVideoPort { get; set; }
        /// <summary>
        /// 0 自动; 3 软解; 4 fmp4; 5 webrtc; 6 hls
        /// </summary>
        public int PlayerMode { get; set; }

        public void SetChannelConf(ChannelConf conf, Setting setting)
        {
            this.RemoteEp = conf.RemoteEp??"";
            this.NickName = conf.NickName;
            this.NetType = conf.NetType;
            this.TalkType = conf.TalkType;
            this.RTVSVideoServer = setting.RTVSVideoServer;
            this.RTVSVideoPort = setting.RTVSVideoPort;
            this.PlayerMode = conf.PlayerMode;
        }
        public void FillChannelConf(ChannelConf conf)
        {
            conf.RemoteEp = this.RemoteEp;
            conf.NickName = this.NickName;
            conf.NetType = this.NetType;
            conf.TalkType = this.TalkType;
            conf.PlayerMode = this.PlayerMode;
        }
        public ChannelConf ToChannelConf()
        {
            var item = new ChannelConf();
            FillChannelConf(item);
            return item;
        }
    }
}
