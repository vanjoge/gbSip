using GB28181.Enums;
using GB28181.MANSRTSP;
using GB28181.XML;
using SIPSorcery.Net;
using SIPSorcery.SIP;
using SQ.Base;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GB28181.Client
{
    public abstract partial class GB28181SipClient
    {
        SQ.Base.ThreadWhile<Object> th;
        private int _sn = 0;
        object lckSN = new object();
        DeviceInfo deviceInfo;
        Dictionary<string, Catalog.Item> ditDevice = new Dictionary<string, Catalog.Item>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="server_id"></param>
        /// <param name="deviceInfo"></param>
        /// <param name="deviceList"></param>
        /// <param name="password"></param>
        /// <param name="expiry"></param>
        /// <param name="UserAgent"></param>
        /// <param name="EnableTraceLogs"></param>
        public GB28181SipClient(string server, string server_id, DeviceInfo deviceInfo, List<Catalog.Item> deviceList, string password = "123456", int expiry = 7200, string UserAgent = "rtvs v1", bool EnableTraceLogs = false) :
            this(new SIPTransport(), server_id, deviceInfo.DeviceID, password, server, expiry)
        {
            this.UserAgent = UserAgent;
            this.deviceInfo = deviceInfo;
            foreach (var item in deviceList)
                ditDevice.Add(item.DeviceID, item);
            if (EnableTraceLogs)
                m_sipTransport.EnableTraceLogs();
        }

        protected SIPTransport SipTransport { get { return m_sipTransport; } }
        protected int GetSN()
        {
            lock (lckSN)
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
        public void Start()
        {
            Stop();

            th = new SQ.Base.ThreadWhile<object>();
            th.SleepMs = 60000;


            m_sipTransport.SIPTransportRequestReceived += SipTransport_SIPTransportRequestReceived;
            m_sipTransport.SIPTransportResponseReceived += SipTransport_SIPTransportResponseReceived;

            this.RegistrationFailed += (uri, err) =>
            {
                SQ.Base.Log.WriteLog4($"{uri}: {err}", LOGTYPE.WARN);
            };
            this.RegistrationTemporaryFailure += (uri, msg) =>
            {
                SQ.Base.Log.WriteLog4($"{uri}: {msg}", LOGTYPE.WARN);
            };
            this.RegistrationRemoved += (uri) =>
            {
                SQ.Base.Log.WriteLog4($"{uri} registration failed.", LOGTYPE.WARN);
            };
            this.RegistrationSuccessful += (uri) =>
            {
                th.StartIfNotRun(Run, null, "Heart");
                SQ.Base.Log.WriteLog4($"{uri} registration succeeded.", LOGTYPE.INFO);
            };

            // Start the thread to perform the initial registration and then periodically resend it.
            this.StartReg();

        }


        public void Stop()
        {
            if (th != null)
            {
                StopReg();
                th.Stop();
                m_sipTransport.Shutdown();
            }
        }

        private async Task SipTransport_SIPTransportResponseReceived(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPResponse sipResponse)
        {
            //if (sipResponse.Status == SIPResponseStatusCodesEnum.Ok)
            //{

            //}
            //switch (sipResponse.Header.CSeqMethod)
            //{
            //    case SIPMethodsEnum.NONE:
            //        break;
            //    case SIPMethodsEnum.UNKNOWN:
            //        break;
            //    case SIPMethodsEnum.REGISTER:
            //        break;
            //    case SIPMethodsEnum.INVITE:
            //        break;
            //    case SIPMethodsEnum.BYE:
            //        break;
            //    case SIPMethodsEnum.ACK:
            //        break;
            //    case SIPMethodsEnum.CANCEL:
            //        break;
            //    case SIPMethodsEnum.OPTIONS:
            //        break;
            //    case SIPMethodsEnum.INFO:
            //        break;
            //    case SIPMethodsEnum.NOTIFY:
            //        break;
            //    case SIPMethodsEnum.SUBSCRIBE:
            //        break;
            //    case SIPMethodsEnum.PUBLISH:
            //        break;
            //    case SIPMethodsEnum.PING:
            //        break;
            //    case SIPMethodsEnum.REFER:
            //        break;
            //    case SIPMethodsEnum.MESSAGE:
            //        break;
            //    case SIPMethodsEnum.PRACK:
            //        break;
            //    case SIPMethodsEnum.UPDATE:
            //        break;
            //    default:
            //        break;
            //}


        }
        private async Task SipTransport_SIPTransportRequestReceived(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
        {
            try
            {
                switch (sipRequest.Header.CSeqMethod)
                {
                    case SIPMethodsEnum.NONE:
                        break;
                    case SIPMethodsEnum.UNKNOWN:
                        break;
                    case SIPMethodsEnum.REGISTER:
                        break;
                    case SIPMethodsEnum.INVITE:
                        await INVITE(sipRequest);
                        break;
                    case SIPMethodsEnum.BYE:
                        if (await On_BYE(sipRequest.Header.From.FromTag, sipRequest))
                        {
                            await SendMessage(sipRequest);
                        }
                        else
                        {
                            await SendMessage(sipRequest, SIPResponseStatusCodesEnum.NotFound);
                        }
                        ;
                        break;
                    case SIPMethodsEnum.ACK:
                        await On_ACK(sipRequest.Header.From.FromTag, sipRequest);
                        break;
                    case SIPMethodsEnum.CANCEL:
                        break;
                    case SIPMethodsEnum.OPTIONS:
                        break;
                    case SIPMethodsEnum.INFO:

                        if (sipRequest.Header.ContentType.IgnoreEquals(Constant.Application_MANSRTSP))
                        {
                            var mrtsp = new MrtspRequest(sipRequest.Body);
                            RTSPResponse rtspres;
                            switch (mrtsp.Method)
                            {
                                case RTSPMethodsEnum.PAUSE:
                                case RTSPMethodsEnum.PLAY:
                                case RTSPMethodsEnum.TEARDOWN:
                                    rtspres = new RTSPResponse(RTSPResponseStatusCodesEnum.OK, null);
                                    break;
                                default:
                                    rtspres = new RTSPResponse(RTSPResponseStatusCodesEnum.BadRequest, null);
                                    break;
                            }
                            rtspres.Header = new RTSPHeader(mrtsp.Header.CSeq, null);


                            var res = GetSIPResponse(sipRequest);
                            res.Header.Contact = new List<SIPContactHeader> { new SIPContactHeader(res.Header.To.ToUserField) };
                            res.Header.ContentType = Constant.Application_MANSRTSP;
                            res.Body = rtspres.ToString();
                            await m_sipTransport.SendResponseAsync(res);

                        }
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
                        if (sipRequest.Header.ContentType.IgnoreEquals(Constant.Application_XML))
                        {
                            XElement bodyXml = XElement.Parse(sipRequest.Body);
                            string cmdType = bodyXml.Element("CmdType")?.Value.ToUpper()!;
                            int sn;
                            if (!int.TryParse(bodyXml.Element("SN")?.Value, out sn))
                            {
                                sn = GetSN();
                            }
                            switch (cmdType)
                            {
                                case "DEVICEINFO":
                                    //查询设备信息
                                    await SendMessage(sipRequest);
                                    await SendDeviceInfo(sn);
                                    break;
                                case "DEVICESTATUS":
                                    //查询设备状态
                                    await SendMessage(sipRequest);
                                    await SendDeviceStatus(sn);
                                    break;
                                case "CATALOG":
                                    await SendMessage(sipRequest);

                                    await SendCatalog(sn, sipRequest);

                                    break;
                                case "RECORDINFO":
                                    await SendMessage(sipRequest);
                                    RecordInfo model = await On_RECORDINFO(SQ.Base.SerializableHelper.DeserializeByStr<RecordInfoQuery>(sipRequest.Body), sipRequest);
                                    if (model != null)
                                    {
                                        var req = GetSIPRequest();
                                        req.Header.CSeq = sipRequest.Header.CSeq;
                                        req.Body = model.ToXmlStr();

                                        await m_sipTransport.SendRequestAsync(req);
                                    }
                                    break;
                            }
                        }
                        break;
                    case SIPMethodsEnum.PRACK:
                        break;
                    case SIPMethodsEnum.UPDATE:
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
            }
        }


        private async Task INVITE(SIPRequest sipRequest)
        {
            SDP28181 ansSdp = await On_INVITE(sipRequest.Header.From.FromTag, SDP28181.NewByStr(sipRequest.Body), sipRequest);
            if (ansSdp != null)
            {
                var res = GetSIPResponse(sipRequest);
                res.Header.Contact = new List<SIPContactHeader> { new SIPContactHeader(res.Header.To.ToUserField) };
                res.Header.ContentType = Constant.Application_SDP;
                res.Header.CSeqMethod = SIPMethodsEnum.INVITE;
                res.Body = ansSdp.GetSdpStr();
                await m_sipTransport.SendResponseAsync(res);
            }
            else
            {
                await SendMessage(sipRequest, SIPResponseStatusCodesEnum.BusyHere);
            }
        }




        /// <summary>
        /// 发送回复确认
        /// </summary>
        /// <param name="sipRequest"></param>
        protected async Task SendMessage(SIPRequest sipRequest, SIPResponseStatusCodesEnum messaageResponse = SIPResponseStatusCodesEnum.Ok)
        {
            var okResponse = GetSIPResponse(sipRequest, messaageResponse);
            await m_sipTransport.SendResponseAsync(okResponse);
        }
        protected async Task SendDeviceInfo(int sn)
        {
            var req = GetSIPRequest();
            deviceInfo.CmdType = CommandType.DeviceInfo;
            deviceInfo.SN = sn;

            req.Body = deviceInfo.ToXmlStr();

            await m_sipTransport.SendRequestAsync(req);

        }
        protected async Task SendDeviceStatus(int sn)
        {
            var req = GetSIPRequest();
            var deviceStatusBody = new DeviceStatus();
            deviceStatusBody.CmdType = CommandType.DeviceStatus;
            //deviceStatusBody.Alarmstatus ;
            deviceStatusBody.Online = "ONLINE";
            deviceStatusBody.DeviceID = deviceInfo.DeviceID;
            deviceStatusBody.Result = "OK";
            deviceStatusBody.Status = "OK";
            deviceStatusBody.SN = sn;

            req.Body = deviceStatusBody.ToXmlStr();

            await m_sipTransport.SendRequestAsync(req);

        }
        protected async Task SendCatalog(int sn, SIPRequest sipRequest)
        {
            var req = GetSIPRequest();
            //req.Header.To.ToURI = sipRequest.Header.From.FromURI;
            //req.Header.From.FromURI = sipRequest.Header.To.ToURI;
            req.Header.CSeq = sipRequest.Header.CSeq;
            var catalogBody = new Catalog();
            catalogBody.CmdType = CommandType.Catalog;
            catalogBody.SumNum = deviceInfo.Channel;
            catalogBody.SN = sn;
            catalogBody.DeviceID = deviceInfo.DeviceID;
            catalogBody.DeviceList = new NList<Catalog.Item>(ditDevice.Values);

            req.Body = catalogBody.ToXmlStr();

            await m_sipTransport.SendRequestAsync(req);

        }



        protected SIPResponse GetSIPResponse(SIPRequest sipRequest, SIPResponseStatusCodesEnum messaageResponse = SIPResponseStatusCodesEnum.Ok)
        {
            SIPResponse res = SIPResponse.GetResponse(sipRequest, messaageResponse, null);
            res.Header.Allow = null;
            res.Header.UserAgent = UserAgent;
            //res.Header.Contact = new List<SIPContactHeader> { new SIPContactHeader(this.UserDisplayName, m_contactURI) };
            //res.Header.CSeq = ++m_cseq;
            //res.Header.CallId = m_callID;
            //res.Header.CallId = sipRequest.Header.CallId;
            //res.Header.CSeq = sipRequest.Header.CSeq;

            return res;
        }
        protected SIPRequest GetSIPRequest(SIPMethodsEnum methodsEnum = SIPMethodsEnum.MESSAGE)
        {
            SIPRequest req = SIPRequest.GetRequest(methodsEnum, m_sipAccountAOR, toSIPToHeader, fromSIPFromHeader);
            req.Header.Allow = null;
            req.Header.ContentType = Constant.Application_XML;


            req.Header.Contact = new List<SIPContactHeader> { new SIPContactHeader(this.UserDisplayName, m_contactURI) };
            req.Header.CSeq = ++m_cseq;
            req.Header.CSeqMethod = methodsEnum;
            req.Header.CallId = m_callID;
            req.Header.UserAgent = UserAgent;
            //req.Header.Expires = m_expiry;

            if (m_customHeaders != null && m_customHeaders.Length > 0)
            {
                foreach (var header in m_customHeaders)
                {
                    req.Header.UnknownHeaders.Add(header);
                }
            }
            return req;
        }



        private void Run(object tag, CancellationToken cancellationToken)
        {
            if (IsRegistered)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var req = GetSIPRequest();
                var keepaliveBody = new KeepAlive();
                keepaliveBody.Status = "OK";
                keepaliveBody.CmdType = CommandType.Keepalive;
                keepaliveBody.SN = GetSN();
                keepaliveBody.DeviceID = deviceInfo.DeviceID;
                req.Body = keepaliveBody.ToXmlStr();
                m_sipTransport.SendRequestAsync(req);
            }
        }



        protected abstract Task<bool> On_BYE(string fromTag, SIPRequest sipRequest);
        protected abstract Task<bool> On_ACK(string fromTag, SIPRequest sipRequest);
        protected abstract Task<SDP28181> On_INVITE(string fromTag, SDP28181 sdp, SIPRequest sipRequest);
        protected abstract Task<RecordInfo> On_RECORDINFO(RecordInfoQuery res, SIPRequest sipRequest);

    }
}
