using GB28181;
using System.Threading.Tasks;
using SQ.Base;
using System.Threading;
using JTServer.Model.RTVS;
using System.Collections.Generic;
using SipServer.Cascade;
using System;
using SipServer.Models.JT;

namespace SipServer.JT2GB
{
    public class JT2GBChannel
    {
        /// <summary>
        /// GB28181通道号
        /// </summary>
        public string Key { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public JTItem JTItem { get; set; }

        JT2GBClient client;
        /// <summary>
        /// 在线状态
        /// </summary>
        private bool online;
        /// <summary>
        /// 绑定的级联客户端列表(引用传递 此类不要做添加删除操作)
        /// </summary>
        private List<CascadeClient> cascadeClients;

        private DateTime heartbeatTime;

        //public JT2GBChannel(JT2GBClient client, string gbChannelId, string sim, int channel, bool is2019)
        //{
        //    this.client = client;
        //    this.sim = sim;
        //    this.channel = channel;
        //    this.is2019 = is2019;
        //    this.online = true;
        //    this.Key = gbChannelId;
        //}
        public JT2GBChannel(JT2GBClient client, JTItem item)
        {
            this.client = client;
            this.JTItem = item;
            //this.sim = item.JTSim;
            //this.channel = item.JTChannel;
            //this.is2019 = item.JTVer == 1;
            this.online = true;
            this.Key = item.GBChannelId;
        }
        private bool Is2019()
        {
            return JTItem.JTVer == 1;
        }


        public async Task<SendRTPTask> INVITE_API(SDP28181 sdp)
        {
            var str = await HttpHelperByHttpClient.HttpRequestHtml(client.manager.sipServer.Settings.RTVSAPI + $"api/GB/CreateSendRTPTask?Protocol={(Is2019() ? "1" : "0")}&Sim={JTItem.JTSim}&Channel={JTItem.JTChannel}&RTPServer={sdp.RtpIp}&RTPPort={sdp.RtpPort}&UseUdp={(sdp.NetType == SDP28181.RTPNetType.TCP ? "false" : "true")}", false, CancellationToken.None);
            return str.ParseJSON<SendRTPTask>();
        }

        protected internal void Offline()
        {
            online = false;
            if (cascadeClients != null && cascadeClients.Count > 0)
            {
                foreach (var item in cascadeClients)
                {
                    item.RemoveChannel(Key);
                }
            }
            cascadeClients = null;
            if (client.manager.ditGroupChannels.TryGetValue(JTItem.GBGroupID, out var lst))
            {
                lock (lst)
                {
                    lst.Remove(this);
                }
            }
        }

        protected internal void Online(JTItem item, List<CascadeClient> lstCascadeClient)
        {
            this.heartbeatTime = DateTime.Now;
            cascadeClients = lstCascadeClient;

            if (item.GBGroupID != JTItem.GBGroupID)
            {
                //移除之前分组记录
                if (client.manager.ditGroupChannels.TryGetValue(item.GBGroupID, out var lst))
                {
                    lock (lst)
                    {
                        lst.Remove(this);
                    }
                }
            }
            JTItem = item;
            //添加当前分组记录
            var lstChannel = client.manager.ditGroupChannels.GetOrAdd(item.GBGroupID, key =>
            {
                return new HashSet<JT2GBChannel>();
            });
            lock (lstChannel)
            {
                lstChannel.Add(this);
            }
        }
        public bool IsOnline()
        {
            return online;
        }
        /// <summary>
        /// 判断心跳超时
        /// </summary>
        /// <returns></returns>
        public bool IsTimeOut()
        {
            if (JTItem.ExpiresIn > 0 && heartbeatTime.DiffNowSec() > JTItem.ExpiresIn)
            {
                Offline();
                return true;
            }
            return false;
        }
    }
}