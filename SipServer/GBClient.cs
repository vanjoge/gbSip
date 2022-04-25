using GB28181;
using GB28181.Enums;
using GB28181.XML;
using SipServer.Models;
using SIPSorcery.SIP;
using SQ.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SipServer
{
    public class GBClient : IDisposable
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

        SIPTransport SipTransport => sipServer.SipTransport;
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
        ConcurrentDictionary<string, Catalog.Item> deviceList = new ConcurrentDictionary<string, Catalog.Item>();
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
            get { return "Dev_" + DeviceId; }
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
        #region 
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
            Off_line();
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
        /// <summary>
        /// 上线处理
        /// </summary>
        /// <returns></returns>
        async Task On_line()
        {
            //从redis获取数据
            var gbdevs = await sipServer.RedisHelper.HashGetAllAsync(redisDevKey);

            foreach (var item in gbdevs)
            {
                if (item.Name == "DeviceInfo" && item.Value.HasValue)
                {
                    deviceInfo = TryParseJSON<DeviceInfo>(item.Value);
                }
                else if (item.Name == "DeviceList" && item.Value.HasValue)
                {
                    var lst = TryParseJSON<List<Catalog.Item>>(item.Value);
                    foreach (var ci in lst)
                    {
                        deviceList.TryAdd(ci.DeviceID, ci);
                    }
                }
                else if (item.Name == "DeviceStatus" && item.Value.HasValue)
                {
                    deviceStatus = TryParseJSON<DeviceStatus>(item.Value);
                }
                else if (item.Name == "Status")
                {
                    var status = TryParseJSON<ConnStatus>(item.Value);
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

            await sipServer.RedisHelper.HashSetAsync(redisDevKey, "Status", Status);

        }
        /// <summary>
        /// 下线处理
        /// </summary>
        /// <returns></returns>
        async Task Off_line()
        {
            Status.Online = false;
            Status.OfflineTime = DateTime.Now;
            await sipServer.RedisHelper.HashSetAsync(redisDevKey, "Status", Status);
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
                        await SipTransport.SendRequestAsync(ack);
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
                await On_line();
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
                await SipTransport.SendResponseAsync(res);
                sipServer.RemoveClient(DeviceId);
                return false;
            }
            else
            {
                res.Header.Expires = 7200;
                res.Header.Date = DateTime.Now.ToTStr();
                await SipTransport.SendResponseAsync(res);
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
                                deviceList[item.DeviceID] = item;
                            }
                            if (deviceList.Count == catalog.SumNum)
                            {
                                //表示收全
                                await sipServer.RedisHelper.HashSetAsync(redisDevKey, "DeviceList", deviceList.Values);
                            }
                            break;
                        case "DEVICEINFO":
                            await SendOkMessage(sipRequest);
                            deviceInfo = SerializableHelper.DeserializeByStr<DeviceInfo>(sipRequest.Body);
                            await sipServer.RedisHelper.HashSetAsync(redisDevKey, "DeviceInfo", deviceInfo);
                            break;
                        case "DEVICESTATUS":
                            await SendOkMessage(sipRequest);
                            deviceStatus = SerializableHelper.DeserializeByStr<DeviceStatus>(sipRequest.Body);
                            await sipServer.RedisHelper.HashSetAsync(redisDevKey, "DeviceStatus", deviceStatus);
                            break;
                        case "RECORDINFO":
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


        #region

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
            await SipTransport.SendRequestAsync(req);
        }
        /// <summary>
        /// 发送OK应答
        /// </summary>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        async Task SendOkMessage(SIPRequest sipRequest)
        {
            SIPResponse okResponse = GetSIPResponse(sipRequest);
            await SipTransport.SendResponseAsync(okResponse);
        }
        #endregion
    }
}
