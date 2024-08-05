using GB28181;
using JTServer.Model.RTVS;
using SQ.Base;
using System;
using System.Threading.Tasks;
using System.Threading;
using SipServer.Models;
using System.Collections.Concurrent;
using GB28181.XML;
using SIPSorcery.SIP;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace SipServer.JT2GB
{
    public class JT2GBClient
    {
        /// <summary>
        /// GB28181设备ID
        /// </summary>
        public string Key { get; protected set; }
        protected internal JT2GBManager manager;
        /// <summary>
        /// 通道信息
        /// </summary>
        protected ConcurrentDictionary<string, JT2GBChannel> ditChannels = new ConcurrentDictionary<string, JT2GBChannel>();
        protected ConcurrentDictionary<string, FromTagCache> ditFromTagCache = new ConcurrentDictionary<string, FromTagCache>();

        public JT2GBClient(string gbDeviceId, JT2GBManager manager)
        {
            this.Key = gbDeviceId;
            this.manager = manager;
        }


        internal JT2GBChannel GetOrAddChannel(string key, Func<string, JT2GBChannel> valueFactory) => ditChannels.GetOrAdd(key, valueFactory);
        internal bool TryGetValue(string key, out JT2GBChannel value) => ditChannels.TryGetValue(key, out value);
        internal bool TryRemove(string key, out JT2GBChannel value) => ditChannels.TryRemove(key, out value);


        /// <summary>
        /// 发起视频
        /// </summary>
        /// <param name="Channel">通道 一般是IPCID</param>
        /// <param name="fromTag"></param>
        /// <param name="SDP"></param>
        /// <returns>1 成功 2需要转换为广播 3已转换为广播并发送</returns>
        public async Task<string> Send_INVITE(string Channel, string fromTag, string SDP, InviteTalk TalkCov)
        {
            var res = await On_INVITE(Channel, fromTag, SDP28181.NewByStr(SDP));
            if (res != null)
                return "1";
            else
                return "0";
        }
        protected async Task<SDP28181> On_INVITE(string did, string fromTag, SDP28181 sdp)
        {
            try
            {
                if (ditChannels.TryGetValue(did, out var channel))
                {
                    var res = await channel.INVITE_API(sdp);
                    if (res.Code == StateCode.Success)
                    {
                        ditFromTagCache[fromTag] = new FromTagCache
                        {
                            TaskID = res.TaskID,
                            sdp = sdp
                        };
                        var ans = sdp.AnsSdp(did, res.LocIP, res.LocIP, res.LocPort);
                        return ans;
                    }
                }
            }
            catch (Exception)
            {

            }
            return null;
        }

        public async Task<bool> On_ACK(string fromTag)
        {
            try
            {
                if (ditFromTagCache.TryGetValue(fromTag, out var item))
                {
                    string url;
                    switch (item.sdp.SType)
                    {
                        case SDP28181.PlayType.Play:
                            if (item.sdp.Media == SDP28181.MediaType.audio)
                                url = $"{manager.sipServer.Settings.RTVSAPI}api/GB/StartRealPlay?TaskID={item.TaskID}&InviteID={fromTag}&SSRC={item.sdp.SSRC}&DataType=3";
                            else
                                url = $"{manager.sipServer.Settings.RTVSAPI}api/GB/StartRealPlay?TaskID={item.TaskID}&InviteID={fromTag}&SSRC={item.sdp.SSRC}";
                            break;
                        case SDP28181.PlayType.Playback:
                            url = $"{manager.sipServer.Settings.RTVSAPI}api/GB/StartPlayback?TaskID={item.TaskID}&InviteID={fromTag}&SSRC={item.sdp.SSRC}&StartTime={item.sdp.TStart.UNIXtoDateTime()}&EndTime={item.sdp.TEnd.UNIXtoDateTime()}";
                            break;
                        case SDP28181.PlayType.Download:
                            url = $"{manager.sipServer.Settings.RTVSAPI}api/GB/StartPlayback?TaskID={item.TaskID}&InviteID={fromTag}&SSRC={item.sdp.SSRC}&StartTime={item.sdp.TStart.UNIXtoDateTime()}&EndTime={item.sdp.TEnd.UNIXtoDateTime()}&DownloadSpeed={item.sdp.Downloadspeed}";
                            break;
                        case SDP28181.PlayType.Talk:
                            url = $"{manager.sipServer.Settings.RTVSAPI}api/GB/StartRealPlay?TaskID={item.TaskID}&InviteID={fromTag}&SSRC={item.sdp.SSRC}&DataType=2";
                            break;
                        default:
                            return false;
                    }
                    var str = await HttpHelperByHttpClient.HttpRequestHtml(url, false, CancellationToken.None);
                    var res = str.ParseJSON<RETModel>();
                    return res.Code == StateCode.Success;
                }
            }
            catch (Exception)
            {

            }
            return false;
        }

        public async Task<bool> On_BYE(string fromTag)
        {
            try
            {
                if (ditFromTagCache.TryGetValue(fromTag, out var item))
                {
                    var str = await SQ.Base.HttpHelperByHttpClient.HttpRequestHtml(manager.sipServer.Settings.RTVSAPI + $"api/GB/Stop?TaskID={item.TaskID}", false, CancellationToken.None);
                    var res = str.ParseJSON<RETModel>();
                    return res.Code == StateCode.Success || res.Code == StateCode.NotFoundTask;
                }
            }
            catch (Exception)
            {

            }
            return false;
        }


        public bool Check()
        {
            var clients = ditChannels.Values.ToList();
            foreach (var item in clients)
            {
                if (item.IsTimeOut())
                {
                    ditChannels.TryRemove(item.Key, out var _);
                }
            }
            return ditChannels.Count > 0;
        }
    }
}