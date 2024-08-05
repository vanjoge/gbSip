using GB28181;
using System.Threading.Tasks;
using SQ.Base;
using System.Threading;
using JTServer.Model.RTVS;
using System.Collections.Generic;
using SipServer.Cascade;
using System;

namespace SipServer.JT2GB
{
    public class JT2GBChannel
    {
        /// <summary>
        /// GB28181通道号
        /// </summary>
        public string Key { get; protected set; }
        /// <summary>
        /// Sim卡号(JT1078)
        /// </summary>
        string sim;
        /// <summary>
        /// 通道号(JT1078)
        /// </summary>
        int channel;
        /// <summary>
        /// 2019版本
        /// </summary>
        bool is2019;

        JT2GBClient client;
        /// <summary>
        /// 在线状态
        /// </summary>
        private bool online;
        /// <summary>
        /// 过期时间(秒) 小于等于0表示不过期
        /// </summary>
        private long expiresIn;
        /// <summary>
        /// 绑定的级联客户端列表
        /// </summary>
        private List<CascadeClient> cascadeClients;

        private DateTime heartbeatTime;

        public JT2GBChannel(JT2GBClient client, string gbChannelId, string sim, int channel, bool is2019)
        {
            this.client = client;
            this.sim = sim;
            this.channel = channel;
            this.is2019 = is2019;
            this.online = true;
            this.Key = gbChannelId;
        }


        public async Task<SendRTPTask> INVITE_API(SDP28181 sdp)
        {
            var str = await HttpHelperByHttpClient.HttpRequestHtml(client.manager.sipServer.Settings.RTVSAPI + $"api/GB/CreateSendRTPTask?Protocol={(is2019 ? "1" : "0")}&Sim={sim}&Channel={channel}&RTPServer={sdp.RtpIp}&RTPPort={sdp.RtpPort}&UseUdp={(sdp.NetType == SDP28181.RTPNetType.TCP ? "false" : "true")}", false, CancellationToken.None);
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
                cascadeClients = null;
            }
        }

        protected internal void Online(long expiresIn, List<CascadeClient> lstCascadeClient)
        {
            this.heartbeatTime = DateTime.Now;
            this.expiresIn = expiresIn;
            cascadeClients = lstCascadeClient;
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
            if (expiresIn > 0 && heartbeatTime.DiffNowSec() > expiresIn)
            {
                Offline();
                return true;
            }
            return false;
        }
    }
}