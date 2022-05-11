using GB28181;
using GB28181.Enums;
using GB28181.MANSRTSP;
using GB28181.XML;
using JX;
using SipServer.Models;
using SIPSorcery.SIP;
using SQ.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SipServer
{
    public partial class GBClient : IDisposable
    {
        #region 变量、字段
        /// <summary>
        /// CmdType判断正则
        /// </summary>
        System.Text.RegularExpressions.Regex regCmdType = new System.Text.RegularExpressions.Regex("<CmdType>(.+)</CmdType>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        public string UserAgent => sipServer.UserAgent;
        string m_callID;
        bool isDispose = false;

        SIPEndPoint _remoteEndPoint;
        /// <summary>
        /// 客户端远端连接信息
        /// </summary>
        public SIPEndPoint RemoteEndPoint
        {
            get { return _remoteEndPoint; }
            protected set
            {
                if (_remoteEndPoint == null || (RemoteEndPoint != value && RemoteEndPoint.Protocol == SIPProtocolsEnum.udp))
                {
                    _remoteEndPoint = value;

                    if (_remoteEndPoint != null)
                    {
                        toSipUri = new SIPURI(toSipUri.Scheme, _remoteEndPoint) { User = toSipUri.User, Parameters = toSipUri.Parameters };
                        toSIPToHeader.ToURI = toSipUri;
                    }
                }
            }
        }

        string LastServerId, LastDeviceId;
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// 上次正常通信时间
        /// </summary>
        public DateTime KeepAliveTime { get; set; }
        public SipServer sipServer;


        int m_cseq, _sn;

        SIPToHeader toSIPToHeader;
        SIPFromHeader fromSIPFromHeader;
        SIPURI toSipUri, m_contactURI;


        /// <summary>
        /// 设备信息
        /// </summary>
        DeviceInfo deviceInfo;
        /// <summary>
        /// 设备目录
        /// </summary>
        DeviceList deviceList = new DeviceList();
        /// <summary>
        /// 设备状态
        /// </summary>
        DeviceStatus deviceStatus;

        object lckCseq = new object();
        object lckSn = new object();
        /// <summary>
        /// 状态
        /// </summary>
        public ConnStatus Status = new ConnStatus();

        string redisDevKey
        {
            get { return RedisConstant.DevInfoHead + DeviceId; }
        }
        #endregion

        #region 构造
        public GBClient(SipServer sipServer, SIPSchemesEnum scheme, SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, string DeviceId, string ServerId, string ServerHost)
        {
            m_contactURI = new SIPURI(scheme, IPAddress.Any, 0);
            toSipUri = new SIPURI(scheme, remoteEndPoint) { User = DeviceId };
            toSIPToHeader = new SIPToHeader(null, toSipUri, null);

            fromSIPFromHeader = new SIPFromHeader(null, new SIPURI(scheme, localSIPEndPoint) { User = ServerId }, CallProperties.CreateNewTag());

            LastServerId = ServerId;
            this.sipServer = sipServer;
            RemoteEndPoint = remoteEndPoint;
            this.DeviceId = DeviceId;
            KeepAliveTime = DateTime.Now;
            m_callID = CallProperties.CreateNewCallId();
            //this.ServerHost = System.Net.IPAddress.TryParse( ServerHost,out var ;


        }
        #endregion

        #region 方法 

        #region 业务处理
        /// <summary>
        /// 上线处理
        /// </summary>
        /// <returns></returns>
        async Task Online()
        {
            //从redis获取数据
            var gbdevs = await sipServer.RedisHelper.HashGetAllAsync(redisDevKey);

            foreach (var entry in gbdevs)
            {
                if (entry.Name == RedisConstant.DeviceInfoKey && entry.Value.HasValue)
                {
                    deviceInfo = TryParseJSON<DeviceInfo>(entry.Value);
                }
                else if (entry.Name == RedisConstant.DeviceListKey && entry.Value.HasValue)
                {
                    var lst = TryParseJSON<List<CatalogItemExtend>>(entry.Value);
                    foreach (var item in lst)
                    {
                        sipServer.SetTree(item.Item.DeviceID, DeviceId);
                        deviceList.AddOrUpdate(item);
                    }
                }
                else if (entry.Name == RedisConstant.DeviceStatusKey && entry.Value.HasValue)
                {
                    deviceStatus = TryParseJSON<DeviceStatus>(entry.Value);
                }
                else if (entry.Name == RedisConstant.StatusKey)
                {
                    var status = TryParseJSON<ConnStatus>(entry.Value);
                    if (status != null)
                    {
                        Status = status;
                    }
                }
            }
            Status.Online = true;
            Status.OnlineTime = DateTime.Now;

            if (deviceInfo == null)
            {
                await Send_GetDevCommand(CommandType.DeviceInfo);
            }
            if (deviceList.Count == 0)
            {
                await Send_GetDevCommand(CommandType.Catalog);
            }

            await Send_GetDevCommand(CommandType.DeviceStatus);

            await sipServer.RedisHelper.HashSetAsync(redisDevKey, RedisConstant.StatusKey, Status);

        }
        /// <summary>
        /// 下线处理
        /// </summary>
        /// <returns></returns>
        async Task Offline()
        {
            Status.Online = false;
            Status.OfflineTime = DateTime.Now;
            await sipServer.RedisHelper.HashSetAsync(redisDevKey, RedisConstant.StatusKey, Status);
            foreach (var item in deviceList.ToList())
            {
                sipServer.RemoveTree(item.Item.DeviceID);
            }
        }
        #endregion

        #region 接收处理
        /// <summary>
        /// Response处理
        /// </summary>
        /// <param name="localSIPEndPoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipResponse"></param>
        /// <returns></returns>
        public async Task OnResponse(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPResponse sipResponse)
        {
            switch (sipResponse.Header.CSeqMethod)
            {
                case SIPMethodsEnum.NONE:
                    break;
                case SIPMethodsEnum.UNKNOWN:
                    break;
                case SIPMethodsEnum.REGISTER:
                    break;
                case SIPMethodsEnum.INVITE:
                    if (sipResponse.Status == SIPResponseStatusCodesEnum.Ok)
                    {
                        if (sipResponse.Header.To.ToTag != null && sipResponse.Header.From.FromTag != null)
                        {
                            if (sipServer.TryGetTag(sipResponse.Header.From.FromTag, out var fromTag))
                            {
                                fromTag.To = sipResponse.Header.To;
                            }
                        }
                        var ack = GetSIPRequest(SIPMethodsEnum.ACK, newHeader: true);
                        ack.Header.From = sipResponse.Header.From;
                        ack.Header.To = sipResponse.Header.To;
                        await SendRequestAsync(ack);
                    }
                    break;
                case SIPMethodsEnum.BYE:
                    sipServer.RemoveTag(sipResponse.Header.From.FromTag);
                    break;
                case SIPMethodsEnum.ACK:
                    break;
                case SIPMethodsEnum.CANCEL:
                    break;
                case SIPMethodsEnum.OPTIONS:
                    break;
                case SIPMethodsEnum.INFO:
                    break;
                case SIPMethodsEnum.NOTIFY:
                    break;
                case SIPMethodsEnum.SUBSCRIBE:
                    break;
                case SIPMethodsEnum.PUBLISH:
                    break;
                case SIPMethodsEnum.PING:
                    break;
                case SIPMethodsEnum.REFER:
                    break;
                case SIPMethodsEnum.MESSAGE:

                    break;
                case SIPMethodsEnum.PRACK:
                    break;
                case SIPMethodsEnum.UPDATE:
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Request处理
        /// </summary>
        /// <param name="localSIPEndPoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        public async Task OnRequest(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
        {
            LastServerId = sipRequest.Header.To.ToURI.User;

            if (RemoteEndPoint != null && RemoteEndPoint != remoteEndPoint &&
                RemoteEndPoint.Protocol == SIPProtocolsEnum.udp
            ) //udp时来源可能变化
            {
                RemoteEndPoint = remoteEndPoint;
            }

            switch (sipRequest.Method)
            {
                case SIPMethodsEnum.REGISTER:
                    if (!await RegisterProcess(remoteEndPoint, sipRequest))
                    {
                        return;
                    }
                    break;
                case SIPMethodsEnum.MESSAGE:
                    await MessageProcess(localSIPEndPoint, remoteEndPoint, sipRequest);
                    break;
                default:
                    break;
            }

            KeepAliveTime = DateTime.Now;
            //此处不严格要求注册认证，有数据上来就认为在线；如果要求注册认证，此处应增加判断
            if (!Status.Online)
            {
                Status.Online = true;
                await Online();
            }

        }


        /// <summary>
        /// 注册处理
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        async Task<bool> RegisterProcess(SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
        {
            var res = GetSIPResponse(sipRequest);
            long expiry = sipRequest.Header.Contact[0].Expires > 0
                ? sipRequest.Header.Contact[0].Expires
                : sipRequest.Header.Expires;
            if (expiry <= 0)
            {
                //注销设备
                res.Header.Expires = 0;
                await SendResponseAsync(res);
                sipServer.RemoveClient(DeviceId);
                return false;
            }
            else
            {
                res.Header.Expires = 7200;
                res.Header.Date = DateTime.Now.ToTStr();
                await SendResponseAsync(res);
                return true;
            }
        }



        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="localSipChannel"></param>
        /// <param name="localSipEndPoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        async Task MessageProcess(SIPEndPoint localSipEndPoint,
           SIPEndPoint remoteEndPoint,
           SIPRequest sipRequest)
        {
            var mth = regCmdType.Match(sipRequest.Body);
            if (mth.Success)
            {
                string cmdType = mth.Groups[1].Value.Trim().ToUpper();

                if (!string.IsNullOrEmpty(cmdType))
                {
                    switch (cmdType)
                    {
                        case "KEEPALIVE": //处理心跳

                            await SendOkMessage(sipRequest);

                            break;
                        case "CATALOG": //处理设备目录
                            await SendOkMessage(sipRequest);
                            var catalog = SerializableHelper.DeserializeByStr<Catalog>(sipRequest.Body);
                            foreach (var item in catalog.DeviceList)
                            {
                                sipServer.SetTree(item.DeviceID, DeviceId);
                                deviceList.AddOrUpdateDeviceList(item);
                            }
                            if (deviceList.Count == catalog.SumNum)
                            {
                                //表示收全
                                await sipServer.RedisHelper.HashSetAsync(redisDevKey, RedisConstant.DeviceListKey, deviceList.ToList());
                            }
                            break;
                        case "DEVICEINFO":
                            await SendOkMessage(sipRequest);
                            deviceInfo = SerializableHelper.DeserializeByStr<DeviceInfo>(sipRequest.Body);
                            await sipServer.RedisHelper.HashSetAsync(redisDevKey, RedisConstant.DeviceInfoKey, deviceInfo);
                            await sipServer.RedisHelper.SortedSetAddAsync(RedisConstant.DeviceIdsKey, DeviceId, Convert.ToDouble(DeviceId));
                            break;
                        case "DEVICESTATUS":
                            await SendOkMessage(sipRequest);
                            deviceStatus = SerializableHelper.DeserializeByStr<DeviceStatus>(sipRequest.Body);
                            await sipServer.RedisHelper.HashSetAsync(redisDevKey, RedisConstant.DeviceStatusKey, deviceStatus);
                            break;
                        case "RECORDINFO":
                            var recordInfo = SerializableHelper.DeserializeByStr<RecordInfo>(sipRequest.Body);

                            if (ditQueryRecordInfo.TryGetValue(recordInfo.SN, out var query))
                            {
                                if (recordInfo.SumNum == 0)
                                {
                                    ditQueryRecordInfo.TryRemove(recordInfo.SN, out query);
                                    await sipServer.RedisHelper.StringSetAsync(RedisConstant.RTVSQueryRecordKey + query.OrderId, new VideoOrderAck
                                    {
                                        Status = 1,
                                        VideoList = new JTVideoListInfo
                                        {
                                            SerialNumber = query.SN808,
                                            FileCount = 0,
                                            FileList = new List<JTVideoFileListItem>()
                                        }
                                    }, new TimeSpan(1, 0, 0));
                                }
                                else
                                {
                                    List<RecordInfo.Item> lst = null;
                                    if (recordInfo.SumNum > recordInfo.RecordList.Count)
                                    {
                                        query.lst.AddRange(recordInfo.RecordList);
                                        if (recordInfo.SumNum <= query.lst.Count)
                                        {
                                            lst = query.lst;
                                        }
                                    }
                                    else
                                    {
                                        lst = recordInfo.RecordList;
                                    }

                                    if (lst != null)
                                    {

                                        var lstFile = new List<JTVideoFileListItem>();
                                        foreach (var item in lst)
                                        {
                                            if (deviceList.TryGetChannel(item.DeviceID, out var channel))
                                            {
                                                uint size = 0;
                                                uint.TryParse(item.FileSize, out size);
                                                lstFile.Add(new JTVideoFileListItem
                                                {
                                                    Channel = channel,
                                                    StartTime = Convert.ToDateTime(item.StartTime),
                                                    EndTime = Convert.ToDateTime(item.EndTime),
                                                    Alarm = 0,
                                                    MediaType = 0,
                                                    StreamType = 1,
                                                    StorageType = MemoryType.MainMemory,
                                                    FileSize = size
                                                });
                                            }
                                        }

                                        var ack = new VideoOrderAck()
                                        {
                                            Status = 1,
                                            VideoList = new JTVideoListInfo
                                            {
                                                FileCount = (uint)lstFile.Count,
                                                FileList = lstFile,
                                                SerialNumber = query.SN808

                                            }
                                        };
                                        await sipServer.RedisHelper.StringSetAsync(RedisConstant.RTVSQueryRecordKey + query.OrderId, ack, new TimeSpan(1, 0, 0));
                                        ditQueryRecordInfo.TryRemove(recordInfo.SN, out query);
                                    }
                                }
                            }
                            await SendOkMessage(sipRequest);
                            break;
                        case "BROADCAST":
                            await SendOkMessage(sipRequest);

                            break;

                        case "MEDIASTATUS":
                            await SendOkMessage(sipRequest);
                            break;
                    }
                }
            }
        }
        #endregion

        #region 发送

        async Task<SocketError> SendResponseAsync(SIPResponse sipResponse, bool waitForDns = false)
        {
            return await sipServer.SipTransport.SendResponseAsync(sipResponse, waitForDns);
        }
        async Task<SocketError> SendRequestAsync(SIPRequest sipRequest, bool waitForDns = false)
        {
            return await sipServer.SipTransport.SendRequestAsync(sipRequest, waitForDns);
        }
        /// <summary>
        /// 发送获取设备信息请求
        /// </summary>
        /// <param name="sipDevice"></param>
        /// <param name="evnt"></param>
        /// <param name="rs"></param>
        /// <param name="timeout"></param>
        async Task Send_GetDevCommand(CommandType commandType)
        {
            var body = new CatalogQuery()
            {
                CmdType = commandType,
                DeviceID = DeviceId,
                SN = GetSN(),
            };
            var req = GetSIPRequest(ContentType: Constant.Application_XML);
            req.Body = body.ToXmlStr();
            await SendRequestAsync(req);
        }
        /// <summary>
        /// 发送OK应答
        /// </summary>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        async Task SendOkMessage(SIPRequest sipRequest)
        {
            SIPResponse okResponse = GetSIPResponse(sipRequest);
            await SendResponseAsync(okResponse);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="head"></param>
        /// <param name="chid"></param>
        /// <param name="qtype"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <returns></returns>
        async Task<int> Send_GetRecordInfo(string chid, string qtype, DateTime dtStart, DateTime dtEnd)
        {
            var body = new RecordInfoQuery()
            {
                DeviceID = chid,
                SN = GetSN(),
                CmdType = CommandType.RecordInfo,
                Secrecy = 0,
                StartTime = dtStart,
                EndTime = dtEnd,
                Type = qtype,
            };
            var req = GetSIPRequest(ContentType: Constant.Application_XML);
            req.Body = body.ToXmlStr();
            await SendRequestAsync(req);
            return body.SN;
        }
        /// <summary>
        /// 发起实时视频
        /// </summary>
        /// <param name="chid"></param>
        /// <param name="ssrc"></param>
        /// <param name="fromTag"></param>
        /// <returns></returns>
        async Task Send_INVITE(string server, int rtpPort, string chid, string ssrc, string fromTag, SDP28181.RTPNetType rtpType = SDP28181.RTPNetType.TCP, SDP28181.PlayType sType = SDP28181.PlayType.Play, bool onlyAudio = false, SDP28181.MediaStreamStatus sendrecv = SDP28181.MediaStreamStatus.recvonly)
        {
            var req = GetSIPRequest(SIPMethodsEnum.INVITE, Constant.Application_SDP, true);
            req.Header.From.FromTag = fromTag;
            req.Header.To.ToURI.User = chid;
            req.Header.To.ToTag = null;
            req.Header.Subject = $"{chid}:{DateTime.Now.Ticks},{req.Header.From.FromURI.User}:0";

            // var ip = Setting.ServerIP; SIPSorcery.Sys.NetServices.GetLocalAddressForRemote(RemoteEndPoint.Address).ToString();
            SDP28181 sdp;
            if (onlyAudio)
            {
                sdp = new SDP28181PCMA();
            }
            else
            {
                sdp = new SDP28181PS();
            }
            sdp.Owner = req.Header.From.FromURI.User;
            //owner = chid,
            sdp.RtpIp = server;
            sdp.RtpPort = rtpPort;
            sdp.NetType = rtpType;
            sdp.SSRC = ssrc;
            sdp.SType = sType;
            sdp.streamStatus = sendrecv;
            sdp.LocalIp = server;
            req.Body = sdp.GetSdpStr();


            sipServer.SetTag(fromTag, new FromTagItem { Client = this, From = req.Header.From, To = req.Header.To });

            await SendRequestAsync(req);
        }

        /// <summary>
        /// 发起历史视频
        /// </summary>
        /// <param name="server"></param>
        /// <param name="rtpPort"></param>
        /// <param name="chid"></param>
        /// <param name="ssrc"></param>
        /// <param name="fromTag"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="rtpType"></param>
        /// <param name="sType"></param>
        /// <param name="onlyAudio"></param>
        /// <returns></returns>
        async Task Send_INVITE_BACK(string server, int rtpPort, string chid, string ssrc, string fromTag, long start, long end, SDP28181.RTPNetType rtpType = SDP28181.RTPNetType.TCP, SDP28181.PlayType sType = SDP28181.PlayType.Playback, bool onlyAudio = false)
        {
            var req = GetSIPRequest(SIPMethodsEnum.INVITE, Constant.Application_SDP, true);
            req.Header.From.FromTag = fromTag;
            req.Header.To.ToURI.User = chid;
            req.Header.To.ToTag = null;
            req.Header.Subject = $"{chid}:{DateTime.Now.Ticks},{req.Header.From.FromURI.User}:0";

            SDP28181 sdp;
            if (onlyAudio)
            {
                sdp = new SDP28181PCMA();
            }
            else
            {
                sdp = new SDP28181PS();
            }
            sdp.Owner = req.Header.From.FromURI.User;
            sdp.RtpIp = server;
            sdp.LocalIp = server;
            sdp.RtpPort = rtpPort;
            sdp.NetType = rtpType;
            sdp.SSRC = ssrc;
            sdp.SType = sType;
            sdp.u = chid + ":0";
            sdp.TStart = start;
            sdp.TEnd = end;

            req.Body = sdp.GetSdpStr();
            sipServer.SetTag(fromTag, new FromTagItem { Client = this, From = req.Header.From, To = req.Header.To });

            await SendRequestAsync(req);
        }

        /// <summary>
        /// 发送视频关闭
        /// </summary>
        /// <param name="fromTag"></param>
        /// <returns></returns>
        async Task Send_Bye(string fromTag)
        {
            SIPRequest req;
            if (sipServer.TryGetTag(fromTag, out var tag))
            {
                req = GetSIPRequest(tag.To, tag.From, SIPMethodsEnum.BYE);
            }
            else
            {
                req = GetSIPRequest(SIPMethodsEnum.BYE, newHeader: true);
                req.Header.From.FromTag = fromTag;
            }
            await SendRequestAsync(req);
        }
        /// <summary>
        /// 发送MANSRTSP
        /// </summary>
        /// <param name="fromTag"></param>
        /// <param name="mrtsp"></param>
        /// <returns></returns>
        async Task Send_MANSRTSP(string fromTag, MrtspRequest mrtsp)
        {
            if (sipServer.TryGetTag(fromTag, out var tag))
            {
                var req = GetSIPRequest(tag.To, tag.From, SIPMethodsEnum.INFO, Constant.Application_MANSRTSP);
                req.Body = mrtsp.ToString();
                await SendRequestAsync(req);
            }
        }
        #endregion

        #region 其他

        /// <summary>
        /// 检查当前是否已超时
        /// </summary>
        /// <returns></returns>
        public bool Check()
        {
            if (KeepAliveTime.DiffNowSec() >= sipServer.Settings.KeepAliveTimeoutSec)
            {
                return false;
            }
            if (RemoteEndPoint.Protocol == SIPProtocolsEnum.tcp)
            {
                return sipServer.sipTCPChannel.HasConnection(RemoteEndPoint);
            }
            return true;
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (isDispose)
            {
                return;
            }
            isDispose = true;
            Offline();
        }
        /// <summary>
        /// 获取Cseq
        /// </summary>
        /// <returns></returns>
        int GetCseq()
        {
            lock (lckCseq)
            {
                if (m_cseq == int.MaxValue)
                {
                    m_cseq = 0;
                }
                else
                {
                    ++m_cseq;
                }
                return m_cseq;
            }
        }
        /// <summary>
        /// 获取SN
        /// </summary>
        /// <returns></returns>
        int GetSN()
        {
            lock (lckSn)
            {
                if (_sn == int.MaxValue)
                {
                    _sn = 0;
                }
                else
                {
                    ++_sn;
                }
                return _sn;
            }
        }
        T TryParseJSON<T>(string strjson)
        {
            try
            {
                return strjson.ParseJSON<T>();
            }
            catch
            {
                return default(T);
            }
        }

        public SIPResponse GetSIPResponse(SIPRequest sipRequest, SIPResponseStatusCodesEnum messaageResponse = SIPResponseStatusCodesEnum.Ok)
        {
            SIPResponse res = SIPResponse.GetResponse(sipRequest, messaageResponse, null);
            res.Header.Allow = null;
            res.Header.UserAgent = UserAgent;

            return res;
        }

        public SIPRequest GetSIPRequest(SIPMethodsEnum methodsEnum = SIPMethodsEnum.MESSAGE, string ContentType = null, bool newHeader = false)
        {

            if (newHeader)
            {
                return GetSIPRequest(new SIPToHeader(toSIPToHeader.ToName, toSIPToHeader.ToURI.CopyOf(), null), new SIPFromHeader(fromSIPFromHeader.FromName, fromSIPFromHeader.FromURI.CopyOf(), null), methodsEnum, ContentType);
            }
            else
            {
                return GetSIPRequest(toSIPToHeader, fromSIPFromHeader, methodsEnum, ContentType);

            }
        }
        public SIPRequest GetSIPRequest(SIPToHeader To, SIPFromHeader From, SIPMethodsEnum methodsEnum = SIPMethodsEnum.MESSAGE, string ContentType = null)
        {
            SIPRequest req;
            req = SIPRequest.GetRequest(methodsEnum, toSipUri, To, From);

            req.Header.Allow = null;
            req.Header.ContentType = ContentType;

            req.Header.Contact = new List<SIPContactHeader> { new SIPContactHeader(null, m_contactURI) };
            req.Header.CSeq = GetCseq();
            req.Header.CSeqMethod = methodsEnum;
            req.Header.CallId = m_callID;
            req.Header.UserAgent = UserAgent;

            return req;
        }

        #endregion
        #endregion
    }
}
