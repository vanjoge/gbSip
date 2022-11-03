using GB28181;
using GB28181.Client;
using GB28181.XML;
using JTServer.Model.RTVS;
using SipServer.Models;
using SIPSorcery.SIP;
using SQ.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SipServer.Cascade
{
    public class CascadeClient : GB28181SipClient
    {
        protected class fromTagCache
        {
            public string TaskID;

            public SDP28181 sdp;
        }
        private CascadeManager manager;
        public string Key { get; protected set; }
        protected ConcurrentDictionary<string, SuperiorChannel> ditChannels = new ConcurrentDictionary<string, SuperiorChannel>();
        protected ConcurrentDictionary<string, fromTagCache> ditFromTagCache = new ConcurrentDictionary<string, fromTagCache>();
        public CascadeClient(CascadeManager manager, string Key, string server, string server_id, DeviceInfo deviceInfo, List<SuperiorChannel> channels, string authUsername = null, string password = "123456", int expiry = 7200, string UserAgent = "rtvs v1", bool EnableTraceLogs = false, double heartSec = 60, double timeOutSec = 300) : base(server, server_id, deviceInfo, null, authUsername, password, expiry, UserAgent, EnableTraceLogs, heartSec, timeOutSec)
        {
            this.Key = Key;
            this.manager = manager;
            List<Catalog.Item> lst = new List<Catalog.Item>();
            foreach (var item in channels)
            {
                var channel_id = item.GetChannelId();
                ditChannels[channel_id] = item;
                var ci = new Catalog.Item
                {
                    DeviceID = channel_id,
                    Name = item.Name,
                    Manufacturer = item.Manufacturer,
                    Model = item.Model,
                    Owner = item.Owner,
                    CivilCode = item.CivilCode,
                    Block = item.Block,
                    Address = item.Address,
                    Parental = item.Parental ? 1 : 0,
                    ParentID = item.ParentId,
                    BusinessGroupID = item.BusinessGroupId,
                    SafetyWay = item.SafetyWay,
                    RegisterWay = item.RegisterWay,
                    CertNum = item.CertNum,
                    Certifiable = item.Certifiable ? 1 : 0,
                    ErrCode = item.ErrCode,
                    EndTime = item.EndTime?.ToTStr(),
                    Secrecy = item.Secrecy ? 1 : 0,
                    IPAddress = item.Ipaddress,
                    Password = item.Password,
                    Status = item.Status,
                    Longitude = item.Longitude,
                    Latitude = item.Latitude,
                };
                if (item.Port > 0)
                    ci.Port = (ushort)item.Port;
                if (item.Longitude > 0)
                    ci.Longitude = item.Longitude;
                if (item.Latitude > 0)
                    ci.Latitude = item.Latitude;
                lst.Add(ci);
            }
            this.ChangeCatalog(lst);
        }

        protected override async Task<bool> On_ACK(string fromTag, SIPRequest sipRequest)
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
                                url = $"{manager.sipServer.Settings.RTVSAPI}api/GB/StartRealPlay?TaskID={item.TaskID}&SSRC={item.sdp.SSRC}&DataType=3";
                            else
                                url = $"{manager.sipServer.Settings.RTVSAPI}api/GB/StartRealPlay?TaskID={item.TaskID}&SSRC={item.sdp.SSRC}";
                            break;
                        case SDP28181.PlayType.Playback:
                        case SDP28181.PlayType.Download:
                            url = $"{manager.sipServer.Settings.RTVSAPI}api/GB/StartPlayback?TaskID={item.TaskID}&SSRC={item.sdp.SSRC}&StartTime={item.sdp.TStart.UNIXtoDateTime()}&EndTime={item.sdp.TEnd.UNIXtoDateTime()}";
                            break;
                        case SDP28181.PlayType.Talk:
                            url = $"{manager.sipServer.Settings.RTVSAPI}api/GB/StartRealPlay?TaskID={item.TaskID}&SSRC={item.sdp.SSRC}&DataType=2";
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

        protected override async Task<bool> On_BYE(string fromTag, SIPRequest sipRequest)
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

        protected override async Task<SDP28181> On_INVITE(string fromTag, SDP28181 sdp, SIPRequest sipRequest)
        {
            try
            {
                var did = sipRequest.Header.To.ToURI.User;
                if (ditChannels.TryGetValue(did, out var channel))
                {
                    var str = await HttpHelperByHttpClient.HttpRequestHtml(manager.sipServer.Settings.RTVSAPI + $"api/GB/CreateSendRTPTask?Protocol=2&Sim={channel.DeviceId}&Channel={did}&RTPServer={sdp.RtpIp}&RTPPort={sdp.RtpPort}&UseUdp={(sdp.NetType == SDP28181.RTPNetType.TCP ? "false" : "true")}", false, CancellationToken.None);

                    var res = str.ParseJSON<SendRTPTask>();
                    if (res.Code == StateCode.Success)
                    {
                        ditFromTagCache[fromTag] = new fromTagCache
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

        protected override async Task<RecordInfo> On_RECORDINFO(RecordInfoQuery res, SIPRequest sipRequest)
        {
            if (ditChannels.TryGetValue(res.DeviceID, out var channel) && manager.sipServer.TryGetClient(channel.DeviceId, out var client))
            {
                var oldsn = res.SN;
                await client.Send_GetRecordInfo(res, p =>
                {
                    p.SN = oldsn;
                    return AnsRecordInfo(sipRequest, p);
                });
                return null;
            }
            else
            {
                return new RecordInfo
                {
                    DeviceID = res.DeviceID,
                    Name = channel?.Name ?? "Unknown",
                    SN = res.SN,
                    SumNum = 0,
                };
            }
        }
    }
}
