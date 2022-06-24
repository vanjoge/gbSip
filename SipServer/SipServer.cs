using SipServer.DB;
using SIPSorcery.SIP;
using SIPSorcery.SIP.App;
using SQ.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SipServer
{
    public class SipServer
    {
        #region 变量、字段
        /// <summary>
        /// 根据FromTag缓存的数据，一般用来响应OnResponse
        /// 未在此列表的OnResponse不做处理
        /// </summary>
        ConcurrentDictionary<string, FromTagItem> ditFromTag = new ConcurrentDictionary<string, FromTagItem>();
        /// <summary>
        /// 客户端列表
        /// </summary>
        ConcurrentDictionary<string, GBClient> ditClient = new ConcurrentDictionary<string, GBClient>();
        /// <summary>
        /// 根据子级获取父级ID
        /// Key:设备目录项ID
        /// Value:设备ID
        /// </summary>
        ConcurrentDictionary<string, string> ditReverseTree = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// SIP监听(包含TCP、UDPV4、UDPV6)
        /// </summary>
        public SIPTransport SipTransport { get; protected set; }


        /// <summary>
        /// SIP TCP监听
        /// </summary>
        public SIPTCPChannel sipTCPChannel { get; protected set; }
        /// <summary>
        /// SIP UserAgent
        /// </summary>
        public string UserAgent = "gbSip v1";

        /// <summary>
        /// 检查线程(在线、超时等检查)
        /// </summary>
        SQ.Base.ThreadWhile<object> thCheck;
        /// <summary>
        /// 配置文件路径
        /// </summary>
        string confPath = SQ.Base.FileHelp.GetMyConfPath() + "Setting.xml";
        /// <summary>
        /// 配置
        /// </summary>
        public Setting Settings { get; protected set; }
        /// <summary>
        /// 当前SSRC值
        /// </summary>
        int nowSSRC = 0;
        object lckSSRC = new object();

        public DBInfo DB;
        #endregion


        class SIPAccount : ISIPAccount
        {

            public string ID { get; set; }

            public string SIPUsername { get; set; }

            public string SIPPassword { get; set; }

            public string HA1Digest { get; set; }

            public string SIPDomain { get; set; }

            public bool IsDisabled { get; set; }
        }
        public SipServer()
        {
            this.Settings = new Setting { };
            try
            {
                if (System.IO.File.Exists(confPath))
                {
                    Settings = SerializableHelper.DeserializeSetting<Setting>(confPath);
                }
                else
                {
                    SerializableHelper.SerializeSetting(Settings, confPath);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog4Ex("SipServer", ex);
            }
        }


        #region 方法

        #region 控制
        /// <summary>
        /// 开启SIP服务
        /// </summary>
        public void Start()
        {
            Log.WriteLog4($"gbSip Starting SipPort:{Settings.SipPort} EnableSipLog:{ Settings.EnableSipLog}");

            DB = new DBInfo(this);
            thCheck = new SQ.Base.ThreadWhile<object>();
            thCheck.SleepMs = 1000;
            thCheck.Start(Check, null, "SipCheck");
            sipTCPChannel = new SIPTCPChannel(new IPEndPoint(IPAddress.Any, Settings.SipPort));
            SipTransport = new SIPTransport();
            SipTransport.AddSIPChannel(new SIPUDPChannel(new IPEndPoint(IPAddress.Any, Settings.SipPort)));
            SipTransport.AddSIPChannel(new SIPUDPChannel(new IPEndPoint(IPAddress.IPv6Any, Settings.SipPort)));
            SipTransport.AddSIPChannel(sipTCPChannel);

            if (Settings.EnableSipLog)
                SipTransport.EnableTraceLogs();

            //_sipTransport.SIP
            SipTransport.SIPTransportRequestReceived += OnRequest;
            SipTransport.SIPTransportResponseReceived += OnResponse;

            Log.WriteLog4($"gbSip Started");
        }


        public delegate void ItemToDo(GBClient item);
        /// <summary>
        /// 遍历Client
        /// </summary>
        /// <param name="itemToDo"></param>
        public void EachClient(ItemToDo itemToDo)
        {
            var clients = ditClient.Values.ToList();
            foreach (var item in clients)
            {
                itemToDo(item);
            }
        }
        /// <summary>
        /// 检查所有Client
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="cancellationToken"></param>
        private void Check(object tag, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EachClient(cl =>
            {
                if (!cl.Check())
                {
                    using (cl)
                    {
                        RemoveClient(cl.DeviceID);
                    }
                }
            });

            //var tags = ditFromTag.Values.ToList();
            //foreach (var item in tags)
            //{
            //    item.AddTime
            //}
        }
        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            Log.WriteLog4($"gbSip Stopping");
            thCheck.Abort();
            thCheck.Join();
            thCheck = null;
            SipTransport.Shutdown();
            SipTransport.Dispose();
            Log.WriteLog4($"gbSip Stopped");
        }
        #endregion

        #region 逻辑 
        public static string GetSIPDomain(string SipServerID)
        {
            return SipServerID.Substring(0, 10);
        }
        public string GetNewSSRC(string chid, bool back)
        {
            lock (lckSSRC)
            {
                return (back ? "1" : "0") + chid.Substring(4, 5) + (++nowSSRC).ToString().StrFixLen(4);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="childID"></param>
        /// <param name="deviceID"></param>
        public void SetTree(string childID, string deviceID)
        {
            ditReverseTree[childID] = deviceID;
        }
        public void RemoveTree(string childID)
        {
            ditReverseTree.TryRemove(childID, out var item);
        }
        /// <summary>
        /// 设置需响应的FromTag
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="item"></param>
        public void SetTag(string tag, FromTagItem item)
        {
            if (tag != null)
                ditFromTag[tag] = item;
        }
        /// <summary>
        /// 移除需响应的FromTag
        /// </summary>
        /// <param name="tag"></param>
        public bool RemoveTag(string tag)
        {
            return ditFromTag.TryRemove(tag, out var item);
        }
        /// <summary>
        /// 获取需响应的FromTag
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryGetTag(string tag, out FromTagItem item)
        {
            return ditFromTag.TryGetValue(tag, out item);
        }
        /// <summary>
        /// 根据SIPRequest获取设备ID
        /// </summary>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        public static string GetSipDeviceId(SIPRequest sipRequest)
        {
            return sipRequest.Header.From.FromURI.User;
        }
        public bool TryGetClient(string key, out GBClient value) => ditClient.TryGetValue(key, out value);
        /// <summary>
        /// 移除Client
        /// </summary>
        /// <param name="key"></param>
        public void RemoveClient(string key, bool updateDB = true)
        {
            if (ditClient.TryRemove(key, out var client))
            {
                client.Dispose(updateDB);
            }
        }
        #endregion

        #region 接收处理
        /// <summary>
        /// 接收Response处理
        /// </summary>
        /// <param name="localSIPEndPoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipResponse"></param>
        /// <returns></returns>
        async Task OnResponse(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPResponse sipResponse)
        {
            if (ditFromTag.TryGetValue(sipResponse.Header.From.FromTag, out var fromTag))
            {
                await fromTag.Client.OnResponse(localSIPEndPoint, remoteEndPoint, sipResponse);
            }
            else
            {
                //非当前INVITE直接BYE
                if (sipResponse.Header.CSeqMethod == SIPMethodsEnum.INVITE)
                {
                    SIPRequest req = SIPRequest.GetRequest(SIPMethodsEnum.BYE, sipResponse.Header.To.ToURI, sipResponse.Header.To, sipResponse.Header.From);
                    req.Header.Allow = null;
                    req.Header.ContentType = null;
                    req.Header.Contact = new List<SIPContactHeader> { new SIPContactHeader(sipResponse.Header.From.FromUserField) };
                    req.Header.CSeq = sipResponse.Header.CSeq;
                    req.Header.CSeqMethod = SIPMethodsEnum.BYE;
                    req.Header.CallId = sipResponse.Header.CallId;
                    req.Header.UserAgent = UserAgent;
                    await SipTransport.SendRequestAsync(req);
                }
            }
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="localSIPEndPoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        async Task OnRequest(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
        {
            try
            {
                string DeviceID = GetSipDeviceId(sipRequest);
                if (sipRequest.Method != SIPMethodsEnum.REGISTER && ditClient.TryGetValue(DeviceID, out var client))
                {
                    await client.OnRequest(localSIPEndPoint, remoteEndPoint, sipRequest);
                }
                else
                {
                    //服务器ID默认从配置取，如未配置则取设备上传值
                    var SipServerID = Settings.SipServerID;
                    if (string.IsNullOrEmpty(SipServerID))
                    {
                        SipServerID = sipRequest.URI.User;
                    }
                    if (await RegisterProcess(localSIPEndPoint, remoteEndPoint, sipRequest, DeviceID, SipServerID))
                    {
                        if (!ditClient.ContainsKey(DeviceID))
                        {
                            client = new GBClient(this, sipRequest.URI.Scheme, localSIPEndPoint, remoteEndPoint, DeviceID, SipServerID, sipRequest.URI.HostAddress);
                            ditClient[DeviceID] = client;
                            await client.Online();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog4Ex("OnRequest", ex);
            }
        }

        /// <summary>
        /// 注册处理
        /// </summary>
        /// <param name="localSIPEndPoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipRequest"></param>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="SipServerID">SIP服务器ID</param>
        /// <returns></returns>
        async Task<bool> RegisterProcess(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest, string DeviceID, string SipServerID)
        {
            SIPResponse res = SIPResponse.GetResponse(sipRequest, SIPResponseStatusCodesEnum.Ok, null);
            res.Header.Allow = null;
            res.Header.UserAgent = UserAgent;
            if (sipRequest.Method != SIPMethodsEnum.REGISTER)
            {
                await Auth(localSIPEndPoint, remoteEndPoint, sipRequest, SipServerID);
                return false;
            }
            long expiry = sipRequest.Header.Expires;
            if (sipRequest.Header.Contact.Count > 0 && sipRequest.Header.Contact[0].Expires > 0)
            {
                expiry = sipRequest.Header.Contact[0].Expires;
            }
            //SIPPassword配置为空不验证 
            if (string.IsNullOrEmpty(Settings.SipPassword) || await Auth(localSIPEndPoint, remoteEndPoint, sipRequest, SipServerID))
            {
                if (expiry <= 0)
                {
                    //注销设备
                    res.Header.Expires = 0;
                    await SipTransport.SendResponseAsync(res);
                    RemoveClient(DeviceID);
                }
                else
                {
                    res.Header.Expires = 7200;
                    res.Header.Date = DateTime.Now.ToTStr();
                    await SipTransport.SendResponseAsync(res);
                    return true;
                }
            }

            return false;
        }

        Task<bool> Auth(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest, string SipServerID)
        {
            var account = new SIPAccount
            {
                SIPDomain = GetSIPDomain(SipServerID),
                SIPPassword = Settings.SipPassword,
                SIPUsername = Settings.SipUsername,
            };
            //配置的SIPUsername为空时直接取上报的Username
            if (string.IsNullOrEmpty(account.SIPUsername) && sipRequest.Header.HasAuthenticationHeader)
            {
                account.SIPUsername = sipRequest.Header.AuthenticationHeaders.First().SIPDigest.Username;
            }
            var authenticationResult = SIPRequestAuthenticator.AuthenticateSIPRequest(localSIPEndPoint, remoteEndPoint, sipRequest, account);
            if (authenticationResult.Authenticated)
            {
                return Task.FromResult(true);
            }
            else
            {
                SIPResponse authReqdResponse = SIPResponse.GetResponse(sipRequest, authenticationResult.ErrorResponse, null);
                authReqdResponse.Header.AuthenticationHeaders.Add(authenticationResult.AuthenticationRequiredHeader);
                authReqdResponse.Header.Allow = null;
                authReqdResponse.Header.UserAgent = UserAgent;
                return SipTransport.SendResponseAsync(authReqdResponse).ContinueWith<bool>(p => { return false; });

            }
        }


        #endregion


        #endregion
    }
}
