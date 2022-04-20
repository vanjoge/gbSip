using GB28181.XML;
using SIPSorcery.SIP;
using SQ.Base;
using System;
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
        public string UserAgent => Sip_Server.UserAgent;
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
        public string DeviceId { get; set; }
        public DateTime KeepAliveTime { get; set; }
        public SipServer Sip_Server;


        private int m_cseq, _sn;

        SIPTransport SipTransport => Sip_Server.SipTransport;
        SIPToHeader toSIPToHeader;
        SIPFromHeader fromSIPFromHeader;
        SIPURI toSipUri, m_contactURI;
        #endregion

        #region 构造
        public GBClient(SipServer sipServer, SIPSchemesEnum scheme, SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, string DeviceId, string ServerId, string ServerHost)
        {
            m_contactURI = new SIPURI(scheme, IPAddress.Any, 0);
            toSipUri = new SIPURI(scheme, remoteEndPoint) { User = DeviceId };
            toSIPToHeader = new SIPToHeader(null, toSipUri, null);

            fromSIPFromHeader = new SIPFromHeader(null, new SIPURI(scheme, localSIPEndPoint) { User = ServerId }, CallProperties.CreateNewTag());

            LastServerId = ServerId;
            Sip_Server = sipServer;
            RemoteEndPoint = remoteEndPoint;
            this.DeviceId = DeviceId;
            KeepAliveTime = DateTime.Now;
            m_callID = CallProperties.CreateNewCallId();
            //this.ServerHost = System.Net.IPAddress.TryParse( ServerHost,out var ;


        }
        #endregion

        #region 方法 
        public bool Check()
        {
            if (KeepAliveTime.DiffNowSec() > 180)
            {
                return false;
            }
            if (RemoteEndPoint.Protocol == SIPProtocolsEnum.tcp)
            {
                return Sip_Server.sipTCPChannel.HasConnection(RemoteEndPoint);
            }
            return true;
        }

        public void Dispose()
        {
            if (isDispose)
            {
                return;
            }
            isDispose = true;

        }
        object lckCseq = new object();
        private int GetCseq()
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
        object lckSn = new object();
        private int GetSn()
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

        #region 接收处理

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
                            if (Sip_Server.TryGetTag(sipResponse.Header.From.FromTag, out var fromTag))
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
                    Sip_Server.RemoveTag(sipResponse.Header.From.FromTag);
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
        public async Task OnRequest(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
        {
            LastServerId = sipRequest.Header.To.ToURI.User;

            if (RemoteEndPoint != null && RemoteEndPoint != remoteEndPoint &&
                RemoteEndPoint.Protocol == SIPProtocolsEnum.udp
            ) //udp时来源可能变化
            {
                RemoteEndPoint = remoteEndPoint;
            }

            {
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
            }

            KeepAliveTime = DateTime.Now;
        }

        /// <summary>
        /// 注册处理
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        private async Task<bool> RegisterProcess(SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
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
                Sip_Server.RemoveClient(DeviceId);
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
        private async Task MessageProcess(SIPEndPoint localSipEndPoint,
            SIPEndPoint remoteEndPoint,
            SIPRequest sipRequest)
        {
            var mth = regCmdType.Match(sipRequest.Body);
            if (mth.Success)
            {
                string cmdType = mth.Groups[1].Value.Trim();

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


                            break;
                        case "DEVICEINFO":
                            await SendOkMessage(sipRequest);
                            //this.deviceInfo = SerializableHelper.DeserializeByStr<DeviceInfo>(sipRequest.Body);

                            break;
                        case "DEVICESTATUS":
                            await SendOkMessage(sipRequest);
                            //this.deviceStatus = SerializableHelper.DeserializeByStr<DeviceStatus>(sipRequest.Body);

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

        private async Task SendOkMessage(SIPRequest sipRequest)
        {
            SIPResponse okResponse = GetSIPResponse(sipRequest);
            await SipTransport.SendResponseAsync(okResponse);
        }
        #endregion

    }
}
