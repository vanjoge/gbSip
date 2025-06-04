using GB28181;
using GB28181.Client;
using GB28181.Enums;
using GB28181.MANSRTSP;
using GB28181.XML;
using JTServer.Model.RTVS;
using SipServer.DBModel;
using SipServer.JT2GB;
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
                if (item.J2GChannel != null)
                {
                    Online = item.J2GChannel.IsOnline();
                }
                else if (client.manager.sipServer.TryGetClient(item.DeviceId, out var gbClient) && gbClient.TryGetChannel(item.ChannelId, out var gbChannel))
                {
                    Online = gbChannel.Data.Online;
                    item.GBChannel = gbChannel;
                }
                else
                {
                    client.manager.ditWaitBindChannel.Add(item);
                }
                item.ChangeOnline(Online);
                if (client.subscribeEnd > DateTime.Now)
                {
                    client.NotifyItem(item.CatalogItem, EventType.ADD);
                }
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
                if (client.subscribeEnd > DateTime.Now)
                {
                    client.NotifyItem(item.CatalogItem, EventType.DEL);
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
                if (client.subscribeEnd > DateTime.Now)
                {
                    client.NotifyItem(item.CatalogItem, EventType.UPDATE);
                }
            }
        }
        private CascadeManager manager;
        public string Key { get; protected set; }
        public string DeviceID { get { return deviceInfo.DeviceID; } }
        public List<string> GroupIds { get; internal protected set; }
        private DateTime subscribeEnd = DateTime.MinValue;
        private string subscribeEvent = "";
        ///// <summary>
        ///// 通道信息
        ///// </summary>
        //protected ConcurrentDictionary<string, SuperiorChannel> ditChannels = new ConcurrentDictionary<string, SuperiorChannel>();
        //TODO:需超时回收
        protected ConcurrentDictionary<string, FromTagCache> ditFromTagCache = new ConcurrentDictionary<string, FromTagCache>();
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
                AddChannel(item, null);
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
                if (ditChild.TryGetValue(did, out var channel))
                {
                    SendRTPTask res;
                    if (channel.J2GChannel != null)
                    {
                        res = await channel.J2GChannel.INVITE_API(sdp);
                    }
                    else
                    {
                        var str = await HttpHelperByHttpClient.HttpRequestHtml(manager.sipServer.Settings.RTVSAPI + $"api/GB/CreateSendRTPTask?Protocol=2&Sim={channel.DeviceId}&Channel={did}&RTPServer={sdp.RtpIp}&RTPPort={sdp.RtpPort}&UseUdp={(sdp.NetType == SDP28181.RTPNetType.TCP ? "false" : "true")}", false, CancellationToken.None);
                        res = str.ParseJSON<SendRTPTask>();
                    }
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

        protected override async Task<RecordInfo> On_RECORDINFO(RecordInfoQuery res, SIPRequest sipRequest)
        {
            if (ditChild.TryGetValue(res.DeviceID, out var channel))
            {
                if (channel.J2GChannel != null)
                {
                    //channel.J2GChannel.
                }
                else if (manager.sipServer.TryGetClient(channel.DeviceId, out var client))
                {
                    var oldsn = res.SN;
                    await client.Send_GetRecordInfo(res, p =>
                    {
                        p.SN = oldsn;
                        return AnsRecordInfo(sipRequest, p);
                    });
                    return null;
                }
            }
            return new RecordInfo
            {
                DeviceID = res.DeviceID,
                Name = channel?.CatalogItem?.Name ?? "Unknown",
                SN = res.SN,
                SumNum = 0,
            };
        }
        protected override async Task<RTSPResponse> On_MANSRTSP(string fromTag, SIPRequest sipRequest)
        {
            try
            {
                if (ditFromTagCache.TryGetValue(fromTag, out var item))
                {
                    if (ditChild.TryGetValue(item.sdp.Owner, out var channel) && manager.sipServer.TryGetClient(channel.DeviceId, out var client))
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

        protected override async Task<SubscribeCatalog> On_SUBSCRIBE(CatalogQuery catalogQuery, SIPRequest sipRequest)
        {
            if (catalogQuery.DeviceID == DeviceID)
            {
                subscribeEnd = DateTime.Now.AddSeconds(sipRequest.Header.Expires);
                subscribeEvent = sipRequest.Header.Event;
                return new SubscribeCatalog
                {
                    SN = catalogQuery.SN,
                    DeviceID = catalogQuery.DeviceID,
                    Result = "OK"
                };
            }
            return null;
        }
        public override void Stop(bool waitStop = true)
        {
            if (IsRun)
            {
                ditChild.ChangeAll(null);
                base.Stop(waitStop);
            }
        }

        protected internal void AddChannel(JT2GBChannel j2gChannel)
        {
            AddChannel(new Models.SuperiorChannel(new TCatalog
            {
                ChannelId = j2gChannel.JTItem.GBChannelId,
                DeviceId = j2gChannel.JTItem.GBDeviceId,
                Name = j2gChannel.JTItem.GBChannelName,
                Manufacturer = "RTVS",
                Model = "gbsip",
                Owner = "Owner",
                CivilCode = j2gChannel.JTItem.GBChannelId.Substring(0, 6),
                Address = "Address",
                RegisterWay = 1,
                Secrecy = false,
                DType = 1001,
                Online = true,
                ParentId = DeviceID + "/" + j2gChannel.JTItem.GBGroupID,
                Status = "ON",
            }, Key, j2gChannel.JTItem.GBChannelId, j2gChannel.JTItem.GBGroupID), j2gChannel);
        }

        protected internal void AddChannel(SuperiorChannel item, JT2GBChannel j2gChannel)
        {
            var channel_id = item.GetChannelId();
            //ditChannels[channel_id] = item;
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
            if (item.IsDevice())
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
            ditChild.AddOrUpdate(new CascadeChannelItem(item.SuperiorId, item.DeviceId, item.ChannelId, ci, j2gChannel));
        }
        protected internal void RemoveChannel(string ChannelId)
        {
            //ditChannels.Remove(ChannelId, out var item);
            ditChild.TryRemove(ChannelId, out var citem);
        }

        private static string Empty2Null(string val)
        {
            return string.IsNullOrEmpty(val) ? null : val;
        }

        private Task NotifyItem(Catalog.Item item, EventType events)
        {
            var catalogBody = new NotifyCatalog();
            catalogBody.SumNum = 1;
            catalogBody.SN = AddCseq();
            catalogBody.DeviceID = deviceInfo.DeviceID;
            var req = GetSIPRequest(SIPMethodsEnum.NOTIFY);
            req.Header.Event = subscribeEvent;
            req.Header.SubscriptionState = "active";
            catalogBody.DeviceList = new NList<NotifyCatalog.Item>(1);
            catalogBody.DeviceList.Add(NotifyCatalog.Item.Copy(item, events));
            req.Body = catalogBody.ToXmlStr();
            return SendRequestAsync(req);
        }
        protected override Task SendNotifyCatalog()
        {
            if (subscribeEnd <= DateTime.Now)
            {
                return Task.CompletedTask;
            }
            //TODO:暂未支持DeviceID与设备ID不一致的场景
            var catalogBody = new NotifyCatalog();
            catalogBody.SumNum = deviceInfo.Channel = ditChild.Count;
            catalogBody.SN = AddCseq();
            catalogBody.DeviceID = deviceInfo.DeviceID;
            if (remoteEndPoint.Protocol == SIPProtocolsEnum.tcp)
            {
                var req = GetSIPRequest(SIPMethodsEnum.NOTIFY);
                req.Header.Event = subscribeEvent;
                req.Header.SubscriptionState = "active";
                catalogBody.DeviceList = new NList<NotifyCatalog.Item>(ditChild.Count);
                foreach (var item in ditChild)
                {
                    catalogBody.DeviceList.Add(NotifyCatalog.Item.Copy(item.Value.CatalogItem, GB28181.Enums.EventType.ADD));
                }
                req.Body = catalogBody.ToXmlStr();

                return SendRequestAsync(req);
            }
            else
            {
                catalogBody.DeviceList = new NList<NotifyCatalog.Item>(1);
                List<NotifyCatalog.Item> waitSend = new List<NotifyCatalog.Item>(ditChild.Count);
                foreach (var item in ditChild)
                {
                    waitSend.Add(NotifyCatalog.Item.Copy(item.Value.CatalogItem, GB28181.Enums.EventType.ADD));
                }
                sendNotifyTask = new SendNotifyCatalogTask(this, catalogBody, waitSend, subscribeEvent);
                return sendNotifyTask.DoSend();
            }
        }
        SendNotifyCatalogTask sendNotifyTask;
        private class SendNotifyCatalogTask
        {
            private NotifyCatalog catalog;
            private List<NotifyCatalog.Item> waitSend;
            public string Event;
            private CascadeClient client;

            public SendNotifyCatalogTask(CascadeClient client, NotifyCatalog catalog, List<NotifyCatalog.Item> waitSend, string subscribeEvent)
            {
                this.client = client;
                this.catalog = catalog;
                this.waitSend = waitSend;
                Event = subscribeEvent;
            }

            public Task DoSend()
            {
                var item = waitSend[0];
                waitSend.RemoveAt(0);
                catalog.DeviceList.Clear();
                catalog.DeviceList.Add(item);


                var req = client.GetSIPRequest(SIPMethodsEnum.NOTIFY);
                req.Header.Event = Event;
                req.Header.SubscriptionState = "active";
                req.Body = catalog.ToXmlStr();
                return client.SendRequestAsync(req);
            }
            public bool NotEmpty()
            {
                return waitSend.Count > 0;
            }
        }
        protected override async Task SipTransport_SIPTransportResponseReceived(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPResponse sipResponse)
        {
            await base.SipTransport_SIPTransportResponseReceived(localSIPEndPoint, remoteEndPoint, sipResponse);


            if (sipResponse.Status == SIPResponseStatusCodesEnum.Ok)
            {
                if (sipResponse.Header.CSeqMethod == SIPMethodsEnum.NOTIFY)
                {
                    if (sendNotifyTask != null
                        //&& sendNotifyTask.Event == sipResponse.Header.Event
                        )
                    {
                        if (sendNotifyTask.NotEmpty())
                        {
                            await sendNotifyTask.DoSend();
                        }
                        else
                        {
                            sendNotifyTask = null;
                        }
                    }
                }
            }
        }
    }
}