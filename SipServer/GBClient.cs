using GB28181;
using GB28181.Enums;
using GB28181.MANSRTSP;
using GB28181.XML;
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

        string LastServerID, LastDeviceID;
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceID { get; set; }
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
        ChannelList channels = new ChannelList();
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
            get { return RedisConstant.DevInfoHead + DeviceID; }
        }
        #endregion

        #region 构造
        public GBClient(SipServer sipServer, SIPSchemesEnum scheme, SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, string DeviceID, string ServerID, string ServerHost)
        {
            m_contactURI = new SIPURI(scheme, IPAddress.Any, 0);
            toSipUri = new SIPURI(scheme, remoteEndPoint) { User = DeviceID };
            toSIPToHeader = new SIPToHeader(null, toSipUri, null);

            fromSIPFromHeader = new SIPFromHeader(null, new SIPURI(scheme, localSIPEndPoint) { User = ServerID }, CallProperties.CreateNewTag());

            LastServerID = ServerID;
            this.sipServer = sipServer;
            RemoteEndPoint = remoteEndPoint;
            this.DeviceID = DeviceID;
            Status.KeepAliveTime = DateTime.Now;
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
            var tuple = await sipServer.DB.GetDevAll(DeviceID);
            deviceInfo = tuple.Item1;
            deviceStatus = tuple.Item3;
            if (tuple.Item2 != null)
            {
                foreach (var item in tuple.Item2)
                {
                    if (item != null && item.DeviceID != null)
                    {
                        sipServer.SetTree(item.DeviceID, DeviceID);
                        channels.AddOrUpdate(item);
                    }
                }
            }
            if (tuple.Item4 != null)
            {
                //Status.Online = tuple.Item4.Online;
                Status.CreateTime = tuple.Item4.CreateTime;
                //Status.OnlineTime = tuple.Item4.OnlineTime;
                Status.OfflineTime = tuple.Item4.OfflineTime;
            }

            Status.Online = true;
            Status.OnlineTime = DateTime.Now;

            if (deviceInfo == null)
            {
                await Send_GetDevCommand(CommandType.DeviceInfo);
            }
            if (channels.Count == 0)
            {
                await Send_GetDevCommand(CommandType.Catalog);
            }

            await Send_GetDevCommand(CommandType.DeviceStatus);

            await sipServer.DB.SaveConnStatus(DeviceID, Status);

        }
        /// <summary>
        /// 下线处理
        /// </summary>
        /// <returns></returns>
        async Task Offline(bool updateDB = true)
        {
            Status.Online = false;
            Status.OfflineTime = DateTime.Now;
            if (updateDB)
            {
                await sipServer.DB.SaveConnStatus(DeviceID, Status);
            }
            foreach (var item in channels.ToList())
            {
                sipServer.RemoveTree(item.DeviceID);
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
                    if (sipResponse.Header.To.ToTag != null && sipResponse.Header.From.FromTag != null)
                    {
                        if (sipServer.TryGetTag(sipResponse.Header.From.FromTag, out var fromTag))
                        {
                            fromTag.To = sipResponse.Header.To;
                        }
                    }
                    if (sipResponse.Status == SIPResponseStatusCodesEnum.Ok)
                    {
                        var ack = GetSIPRequest(SIPMethodsEnum.ACK, newHeader: true);
                        ack.Header.CSeq = sipResponse.Header.CSeq;
                        ack.Header.From = sipResponse.Header.From;
                        ack.Header.To = sipResponse.Header.To;
                        await SendRequestAsync(ack);
                    }
                    else
                    {
                        await Send_Bye(sipResponse.Header.From.FromTag);
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
            LastServerID = sipRequest.Header.To.ToURI.User;

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
            Status.KeepAliveTime = DateTime.Now;
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
                sipServer.RemoveClient(DeviceID);
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
                                sipServer.SetTree(item.DeviceID, DeviceID);
                                channels.AddOrUpdate(item);
                            }
                            if (channels.Count == catalog.SumNum)
                            {
                                //表示收全
                                await sipServer.DB.SaveChannels(DeviceID, channels.ToList());
                            }
                            break;
                        case "DEVICEINFO":
                            await SendOkMessage(sipRequest);
                            deviceInfo = SerializableHelper.DeserializeByStr<DeviceInfo>(sipRequest.Body);
                            await sipServer.DB.SaveDeviceInfo(deviceInfo);
                            break;
                        case "DEVICESTATUS":
                            await SendOkMessage(sipRequest);
                            deviceStatus = SerializableHelper.DeserializeByStr<DeviceStatus>(sipRequest.Body);
                            await sipServer.DB.SaveDeviceStatus(DeviceID, deviceStatus);
                            break;
                        case "RECORDINFO":
                            var recordInfo = SerializableHelper.DeserializeByStr<RecordInfo>(sipRequest.Body);
                            await AnsRTVSGetRecordInfo(recordInfo);
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
                DeviceID = DeviceID,
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

        public async Task<int> Send_GetRecordInfo(string OrderID, RecordInfoQuery query)
        {
            var req = GetSIPRequest(ContentType: Constant.Application_XML);
            var nowsn = GetSN();
            ditQueryRecordInfo[nowsn] = new QueryRecordInfo
            {
                SNOld = query.SN,
                OrderID = OrderID
            };
            query.SN = nowsn;
            req.Body = query.ToXmlStr();
            await SendRequestAsync(req);
            return query.SN;
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
        public async Task<int> Send_GetRecordInfo(string chid, string qtype, DateTime dtStart, DateTime dtEnd)
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
        /// 发起视频
        /// </summary>
        /// <param name="Channel">通道 一般是IPCID</param>
        /// <param name="fromTag"></param>
        /// <param name="SDP"></param>
        /// <returns></returns>
        public async Task Send_INVITE(string Channel, string fromTag, string SDP)
        {
            var req = GetSIPRequest(SIPMethodsEnum.INVITE, Constant.Application_SDP, true);
            req.Header.From.FromTag = fromTag;
            req.Header.To.ToURI.User = Channel;
            req.Header.To.ToTag = null;
            req.Header.Subject = $"{Channel}:{DateTime.Now.Ticks},{req.Header.From.FromURI.User}:0";
            req.Body = SDP;
            sipServer.SetTag(fromTag, new FromTagItem { Client = this, From = req.Header.From, To = req.Header.To });

            await SendRequestAsync(req);
        }
        /// <summary>
        /// 发送视频关闭
        /// </summary>
        /// <param name="fromTag"></param>
        /// <returns></returns>
        public async Task Send_Bye(string fromTag)
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
        public async Task<bool> Send_MANSRTSP(string fromTag, string mrtsp)
        {
            if (sipServer.TryGetTag(fromTag, out var tag))
            {
                var req = GetSIPRequest(tag.To, tag.From, SIPMethodsEnum.INFO, Constant.Application_MANSRTSP);
                req.Body = mrtsp;
                await SendRequestAsync(req);
                return true;
            }
            return false;
        }

        #endregion

        #region 其他

        /// <summary>
        /// 检查当前是否已超时
        /// </summary>
        /// <returns></returns>
        public bool Check()
        {
            if (Status.KeepAliveTime.DiffNowSec() >= sipServer.Settings.KeepAliveTimeoutSec)
            {
                return false;
            }
            if (RemoteEndPoint.Protocol == SIPProtocolsEnum.tcp)
            {
                return sipServer.sipTCPChannel.HasConnection(RemoteEndPoint);
            }
            return true;
        }
        public void Dispose()
        {
            Dispose(true);
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose(bool updateDB)
        {
            if (isDispose)
            {
                return;
            }
            isDispose = true;
            Offline(updateDB);
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

        public DeviceInfoExt GetDeviceInfoExt()
        {
            return new DeviceInfoExt
            {
                Device = deviceInfo,
                Status = Status,
                RemoteEndPoint = RemoteEndPoint,
            };
        }
        #endregion
        #endregion
    }
}
