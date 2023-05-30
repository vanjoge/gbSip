using GB28181.Enums;
using GB28181.MANSRTSP;
using GB28181.XML;
using Newtonsoft.Json.Linq;
using SIPSorcery.Net;
using SIPSorcery.SIP;
using SQ.Base;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GB28181.Client
{
    public abstract partial class GB28181SipClient<T> where T : ChannelItem
    {
        protected SQ.Base.ThreadWhile<Object> th;
        private int _sn = 0;
        object lckSN = new object();
        protected DeviceInfo deviceInfo;
        /// <summary>
        /// 下挂通道/设备列表
        /// </summary>
        protected NotifyChangeDictionary<string, T> ditChild;

        DateTime LastHeartTime = DateTime.MinValue, LastAnsOKTime;
        protected SIPEndPoint remoteEndPoint;
        double m_heartSec, m_timeOutSec;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="server_id"></param>
        /// <param name="deviceInfo"></param>
        /// <param name="ditChild"></param>
        /// <param name="password"></param>
        /// <param name="expiry"></param>
        /// <param name="UserAgent"></param>
        /// <param name="EnableTraceLogs"></param>
        public GB28181SipClient(string server, string server_id, DeviceInfo deviceInfo, string authUsername = null, string password = "123456", int expiry = 7200, string UserAgent = "rtvs v1", bool EnableTraceLogs = false, double heartSec = 60, double timeOutSec = 300) :
            this(new SIPTransport(), server_id, deviceInfo.DeviceID, password, server, expiry)
        {
            this.UserAgent = UserAgent;
            this.deviceInfo = deviceInfo;
            this.m_heartSec = heartSec;
            this.m_timeOutSec = timeOutSec;
            if (!string.IsNullOrEmpty(authUsername))
            {
                this.m_authUsername = authUsername;
            }
            if (EnableTraceLogs)
                m_sipTransport.EnableTraceLogs();

            remoteEndPoint = new SIPEndPoint(m_sipAccountAOR);

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
            th.SleepMs = 1000;


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
                LastAnsOKTime = DateTime.Now;
                th?.StartIfNotRun(Run, null, "Check");
                SQ.Base.Log.WriteLog4($"{uri} registration succeeded.", LOGTYPE.INFO);
            };

            // Start the thread to perform the initial registration and then periodically resend it.
            this.StartReg();

        }
        public bool IsRun
        {
            get
            {
                return th != null;
            }
        }
        public virtual void Stop(bool waitStop = true)
        {
            if (IsRun)
            {
                StopReg();
                if (waitStop)
                {
                    th.Stop();
                }
                else
                {
                    th.Abort();
                }
                th = null;
                Shutdown();
            }
        }
        Task Shutdown()
        {
            return Task.Run(() =>
            {
                // Allow for unregister request to be sent(REGISTER with 0 expiry)
                Task.Delay(1500).Wait();
                foreach (var item in m_sipTransport.GetSIPChannels())
                {
                    if (item.IsProtocolSupported(SIPProtocolsEnum.tcp))
                    {
                        //TCP时直接关闭会造成死锁，在关闭后需要模拟一个TCP连接连到本地监听，防止死锁
                        Task.Run(() =>
                        {
                            Task.Delay(100).Wait();
                            using (var conn = new Network.TCPChannel("127.0.0.1", item.Port))
                            {
                                conn.Connect();
                            }
                        });
                        item.Dispose();
                    }
                }
                m_sipTransport.Shutdown();
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="localSIPEndPoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipResponse"></param>
        /// <returns></returns>
        private async Task SipTransport_SIPTransportResponseReceived(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPResponse sipResponse)
        {
            if (sipResponse.Status == SIPResponseStatusCodesEnum.Ok)
            {
                LastAnsOKTime = DateTime.Now;
            }
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
                            var rtspres = await On_MANSRTSP(sipRequest.Header.From.FromTag, sipRequest);
                            if (rtspres != null)
                            {
                                var res = GetSIPResponse(sipRequest);
                                res.Header.Contact = new List<SIPContactHeader> { new SIPContactHeader(res.Header.To.ToUserField) };
                                res.Header.ContentType = Constant.Application_MANSRTSP;
                                res.Body = rtspres.ToString();
                                await m_sipTransport.SendResponseAsync(res);
                            }
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
                                    var deviceStatusQuery = SQ.Base.SerializableHelper.DeserializeByStr<DeviceStatusQuery>(sipRequest.Body);
                                    await SendDeviceStatus(sn, deviceStatusQuery.DeviceID);
                                    break;
                                case "CATALOG":
                                    await SendMessage(sipRequest);
                                    var catalogQuery = SQ.Base.SerializableHelper.DeserializeByStr<CatalogQuery>(sipRequest.Body);

                                    await SendCatalog(catalogQuery, sipRequest);

                                    break;
                                case "RECORDINFO":
                                    await SendMessage(sipRequest);
                                    RecordInfo model = await On_RECORDINFO(SQ.Base.SerializableHelper.DeserializeByStr<RecordInfoQuery>(sipRequest.Body), sipRequest);
                                    if (model != null)
                                    {
                                        await AnsRecordInfo(sipRequest, model);
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
        protected virtual Task SendMessage(SIPRequest sipRequest, SIPResponseStatusCodesEnum messaageResponse = SIPResponseStatusCodesEnum.Ok)
        {
            var okResponse = GetSIPResponse(sipRequest, messaageResponse);
            return m_sipTransport.SendResponseAsync(okResponse);
        }
        protected virtual Task SendDeviceInfo(int sn)
        {
            var req = GetSIPRequest();
            deviceInfo.CmdType = CommandType.DeviceInfo;
            deviceInfo.SN = sn;
            deviceInfo.Channel = ditChild.Count;

            req.Body = deviceInfo.ToXmlStr();

            return m_sipTransport.SendRequestAsync(req);

        }
        protected virtual Task SendDeviceStatus(int sn, string DeviceID)
        {
            var req = GetSIPRequest();
            DeviceStatus deviceStatusBody;
            if (DeviceID == deviceInfo.DeviceID)
            {
                deviceStatusBody = new DeviceStatus();
                //deviceStatusBody.CmdType = CommandType.DeviceStatus;
                //deviceStatusBody.Alarmstatus ;
                deviceStatusBody.Online = "ONLINE";
                deviceStatusBody.DeviceID = deviceInfo.DeviceID;
                deviceStatusBody.Status = "OK";
            }
            else if (ditChild.TryGetValue(DeviceID, out var channelItem))
            {
                deviceStatusBody = channelItem.Status;
            }
            else
            {
                return Task.FromResult(false);
            }
            deviceStatusBody.Result = "OK";
            deviceStatusBody.SN = sn;

            req.Body = deviceStatusBody.ToXmlStr();

            return m_sipTransport.SendRequestAsync(req);

        }
        protected virtual Task SendCatalog(CatalogQuery query, SIPRequest sipRequest)
        {
            //TODO:暂未支持DeviceID与设备ID不一致的场景
            var req = GetSIPRequest();
            req.Header.CSeq = sipRequest.Header.CSeq;
            var catalogBody = new Catalog();
            catalogBody.SumNum = deviceInfo.Channel = ditChild.Count;
            catalogBody.SN = query.SN;
            catalogBody.DeviceID = deviceInfo.DeviceID;
            catalogBody.DeviceList = new NList<Catalog.Item>(ditChild.Count);
            foreach (var item in ditChild)
            {
                catalogBody.DeviceList.Add(item.Value.CatalogItem);
            }
            req.Body = catalogBody.ToXmlStr();

            return m_sipTransport.SendRequestAsync(req);

        }

        protected Task AnsRecordInfo(SIPRequest sipRequest, RecordInfo model)
        {
            var req = GetSIPRequest();
            req.Header.CSeq = sipRequest.Header.CSeq;
            req.Body = model.ToXmlStr();

            return m_sipTransport.SendRequestAsync(req);
        }

        //public void ChangeCatalog(IEnumerable<ChannelItem> deviceList)
        //{
        //    ditDevice.ChangeAll(deviceList);
        //    deviceInfo.Channel = ditDevice.Count;
        //}


        protected virtual SIPResponse GetSIPResponse(SIPRequest sipRequest, SIPResponseStatusCodesEnum messaageResponse = SIPResponseStatusCodesEnum.Ok)
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
        protected virtual SIPRequest GetSIPRequest(SIPMethodsEnum methodsEnum = SIPMethodsEnum.MESSAGE)
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
            if (m_isRegistered)
            {
                cancellationToken.ThrowIfCancellationRequested();


                var channels = m_sipTransport.GetSIPChannels();
                if (channels.Count < 1
                    ||
                    (remoteEndPoint.Protocol == SIPProtocolsEnum.tcp && !channels[0].HasConnection(remoteEndPoint))
                    || LastAnsOKTime.DiffNowSec() >= m_timeOutSec
                    )
                {
                    ReStartReg();
                }
                else if (LastHeartTime.DiffNowSec() >= m_heartSec)
                {
                    LastHeartTime = DateTime.Now;
                    var req = GetSIPRequest();
                    var keepaliveBody = new KeepAlive();
                    keepaliveBody.Status = "OK";
                    keepaliveBody.CmdType = CommandType.Keepalive;
                    keepaliveBody.SN = GetSN();
                    keepaliveBody.DeviceID = deviceInfo.DeviceID;
                    req.Body = keepaliveBody.ToXmlStr();
                    var err = m_sipTransport.SendRequestAsync(req).GetAwaiter().GetResult();
                    if (err != System.Net.Sockets.SocketError.Success)
                    {
                        ReStartReg();
                    }
                }
            }
        }
        private void ReStartReg()
        {
            StopReg(false);
            m_isRegistered = false;
            StartReg();
        }



        protected abstract Task<bool> On_BYE(string fromTag, SIPRequest sipRequest);
        protected abstract Task<bool> On_ACK(string fromTag, SIPRequest sipRequest);
        protected abstract Task<SDP28181> On_INVITE(string fromTag, SDP28181 sdp, SIPRequest sipRequest);
        protected abstract Task<RecordInfo> On_RECORDINFO(RecordInfoQuery res, SIPRequest sipRequest);

        protected abstract Task<RTSPResponse> On_MANSRTSP(string fromTag, SIPRequest sipRequest);
    }
}
