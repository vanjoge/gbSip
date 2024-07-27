using GB28181;
using GB28181.Client;
using GB28181.MANSRTSP;
using GB28181.XML;
using JTServer.Model.RTVS;
using SipServer.Models;
using SIPSorcery.Net;
using SIPSorcery.SIP;
using SQ.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SipServer.Cascade
{
    public class CascadeClient : GB28181SipClient<CascadeChannelItem>
    {
        public class CascadeChannelDictionary : NotifyChangeDictionary<string, CascadeChannelItem>
        {
            public CascadeChannelDictionary(CascadeClient client)
            {
                this.client = client;

            }
            protected CascadeClient client;
            protected override string GetKey(CascadeChannelItem item)
            {
                return item.CatalogItem.DeviceID;
            }

            protected override void OnChannelItemAdd(CascadeChannelItem item)
            {
                bool Online = false;
                if (client.manager.sipServer.TryGetClient(item.DeviceId, out var gbClient) && gbClient.TryGetChannel(item.ChannelId, out var gbChannel))
                {
                    Online = gbChannel.Data.Online;
                    item.GBChannel = gbChannel;
                }
                else
                {
                    client.manager.ditWaitBindChannel.Add(item);
                }
                item.ChangeOnline(Online);
            }

            protected override void OnChannelItemRemove(CascadeChannelItem item)
            {
                if (item.GBChannel == null)
                {
                    client.manager.ditWaitBindChannel.Remove(item);
                }
                else
                {
                    item.GBChannel.ditCascadeChannel.TryRemove(item.SuperiorId, out var val);
                    item.GBChannel = null;
                }
            }

            protected override void OnChannelItemUpdate(CascadeChannelItem old, CascadeChannelItem item)
            {
                if (old.GBChannel == null)
                {
                    client.manager.ditWaitBindChannel.Update(old, item);
                }
                else
                {
                    item.GBChannel = old.GBChannel;
                    old.GBChannel = null;
                    item.GBChannel.ditCascadeChannel[item.SuperiorId] = item;
                }
            }
        }
        protected class fromTagCache
        {
            public string TaskID;

            public SDP28181 sdp;
        }
        private CascadeManager manager;
        public string Key { get; protected set; }
        /// <summary>
        /// 通道信息
        /// </summary>
        protected ConcurrentDictionary<string, SuperiorChannel> ditChannels = new ConcurrentDictionary<string, SuperiorChannel>();
        protected ConcurrentDictionary<string, fromTagCache> ditFromTagCache = new ConcurrentDictionary<string, fromTagCache>();
        public CascadeClient(CascadeManager manager, string Key, string server, string server_id, DeviceInfo deviceInfo, List<SuperiorChannel> channels, string authUsername = null, string password = "123456", int expiry = 7200, string UserAgent = "rtvs v1", bool EnableTraceLogs = false, double heartSec = 60, double timeOutSec = 300, int localPort = 0)
            : base(server, server_id, deviceInfo, authUsername, password, expiry, UserAgent, EnableTraceLogs, heartSec, timeOutSec)
        {
            if (localPort > 0)
            {
                if (remoteEndPoint.Protocol == SIPProtocolsEnum.tcp)
                {
                    SipTransport.AddSIPChannel(new SIPTCPChannel(new IPEndPoint(IPAddress.Any, localPort)));
                }
                else
                {
                    SipTransport.AddSIPChannel(new SIPUDPChannel(new IPEndPoint(IPAddress.Any, localPort)));
                }
            }
            this.Key = Key;
            this.manager = manager;
            this.ditChild = new CascadeChannelDictionary(this);
            foreach (var item in channels)
            {
                AddChannel(item);
            }
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
        protected override async Task<RTSPResponse> On_MANSRTSP(string fromTag, SIPRequest sipRequest)
        {
            try
            {
                if (ditFromTagCache.TryGetValue(fromTag, out var item))
                {
                    if (ditChannels.TryGetValue(item.sdp.Owner, out var channel) && manager.sipServer.TryGetClient(channel.DeviceId, out var client))
                    {
                        if (await client.Send_MANSRTSP(fromTag, sipRequest.Body, async p =>
                        {
                            var res = GetSIPResponse(sipRequest);
                            res.Header.Contact = new List<SIPContactHeader> { new SIPContactHeader(res.Header.To.ToUserField) };
                            res.Header.ContentType = Constant.Application_MANSRTSP;
                            //res.Header.CSeqMethod = SIPMethodsEnum.INFO;
                            res.Body = p.Body;
                            await SipTransport.SendResponseAsync(res);
                        }))
                        {
                            return null;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            var mrtsp = new MrtspRequest(sipRequest.Body);
            RTSPResponse rtspres = new RTSPResponse(RTSPResponseStatusCodesEnum.NotFound, null);
            rtspres.Header = new RTSPHeader(mrtsp.Header.CSeq, null);
            return rtspres;
        }

        public override void Stop(bool waitStop = true)
        {
            if (IsRun)
            {
                ditChild.ChangeAll(null);
                base.Stop(waitStop);
            }
        }

        protected internal void AddChannel(SuperiorChannel item)
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
                Block = Empty2Null(item.Block),
                Address = item.Address,
                ParentID = item.ParentId,
                BusinessGroupID = Empty2Null(item.BusinessGroupId),
                RegisterWay = item.RegisterWay,
                Secrecy = item.Secrecy ? 1 : 0,
                IPAddress = Empty2Null(item.Ipaddress),
                Password = Empty2Null(item.Password),
                Status = item.Status,
            };
            if (item.DType < 1 || item.DType > 3)
            {
                ci.Parental = item.Parental ? 1 : 0;
                ci.SafetyWay = item.SafetyWay;
            }
            if (item.EndTime.HasValue)
            {
                ci.CertNum = item.CertNum;
                ci.Certifiable = item.Certifiable ? 1 : 0;
                ci.ErrCode = item.ErrCode;
                ci.EndTime = item.EndTime.Value.ToTStr();
            }
            if (item.Port > 0)
                ci.Port = (ushort)item.Port;
            if (item.Longitude > 0)
                ci.Longitude = item.Longitude;
            if (item.Latitude > 0)
                ci.Latitude = item.Latitude;
            ditChild.AddOrUpdate(new CascadeChannelItem(item.SuperiorId, item.DeviceId, item.ChannelId, ci));
        }
        protected internal void RemoveChannel(string ChannelId)
        {
            ditChannels.Remove(ChannelId, out var item);
            ditChild.TryRemove(ChannelId, out var citem);
        }

        private static string Empty2Null(string val)
        {
            return string.IsNullOrEmpty(val) ? null : val;
        }
    }
}
