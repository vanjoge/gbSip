using SIPSorcery.SIP;
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
        public SIPTransport SipTransport { get; protected set; }
        public SIPTCPChannel sipTCPChannel { get; protected set; }

        public string UserAgent = "gbSip v1";


        SQ.Base.ThreadWhile<object> thCheck;

        public Setting Settings { get; protected set; }

        string confPath = SQ.Base.FileHelp.GetMyConfPath() + "Setting.xml";
        #endregion


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

        public void SetTag(string tag, FromTagItem item)
        {
            if (tag != null)
                ditFromTag[tag] = item;
        }
        public void RemoveTag(string tag)
        {
            ditFromTag.TryRemove(tag, out var item);
        }
        public bool TryGetTag(string tag, out FromTagItem item)
        {
            return ditFromTag.TryGetValue(tag, out item);
        }

        #region 控制
        public void Start()
        {

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

        }


        public delegate void ItemToDo(GBClient item);
        public void EachClient(ItemToDo itemToDo)
        {
            var clients = ditClient.Values.ToList();
            foreach (var item in clients)
            {
                itemToDo(item);
            }
        }

        private void Check(object tag, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EachClient(cl =>
            {
                if (!cl.Check())
                {
                    using (cl)
                    {
                        RemoveClient(cl.DeviceId);
                    }
                }
            });

            //var tags = ditFromTag.Values.ToList();
            //foreach (var item in tags)
            //{
            //    item.AddTime
            //}
        }

        public void Stop()
        {
            thCheck.Abort();
            thCheck.Join();
            thCheck = null;

            SipTransport.Shutdown();
            SipTransport.Dispose();
        }
        #endregion


        #region 逻辑  
        public static string GetSipDeviceId(SIPRequest sipRequest)
        {
            return sipRequest.Header.From.FromURI.User;
        }
        GBClient GetSipClient(SIPRequest sipRequest)
        {
            ditClient.TryGetValue(GetSipDeviceId(sipRequest), out var client);
            return client;
        }

        public void RemoveClient(string key)
        {
            if (ditClient.TryRemove(key, out var client))
            {
                client.Dispose();
            }
        }
        #endregion

        #region 接收处理
        async Task OnResponse(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPResponse sipResponse)
        {
            if (ditFromTag.TryGetValue(sipResponse.Header.From.FromTag, out var fromTag))
            {
                await fromTag.Client.OnResponse(localSIPEndPoint, remoteEndPoint, sipResponse);
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

                var client = ditClient.GetOrAdd(GetSipDeviceId(sipRequest), K =>
                {
                    return new GBClient(this, sipRequest.URI.Scheme, localSIPEndPoint, remoteEndPoint, K, sipRequest.Header.To.ToURI.User, sipRequest.URI.HostAddress);
                });
                await client.OnRequest(localSIPEndPoint, remoteEndPoint, sipRequest);


            }
            catch (Exception ex)
            {
                Log.WriteLog4Ex("OnRequest", ex);
            }
        }



        #endregion
    }
}
