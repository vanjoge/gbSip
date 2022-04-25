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
        /// Redis操作类
        /// </summary>
        public RedisHelp.RedisHelper RedisHelper;
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


        #region 方法

        #region 控制
        /// <summary>
        /// 开启SIP服务
        /// </summary>
        public void Start()
        {
            Console.WriteLine($"ServerIP:{Settings.ServerIP}");
            Console.WriteLine($"SipPort:{Settings.SipPort}");
            Console.WriteLine($"EnableSipLog:{Settings.EnableSipLog}");

            RedisHelper = new RedisHelp.RedisHelper(-1, Settings.RedisExchangeHosts);
            RedisHelper.SetSysCustomKey("GB_");

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
        /// <summary>
        /// 停止
        /// </summary>
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
        public void RemoveTag(string tag)
        {
            ditFromTag.TryRemove(tag, out var item);
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
        GBClient GetSipClient(SIPRequest sipRequest)
        {
            ditClient.TryGetValue(GetSipDeviceId(sipRequest), out var client);
            return client;
        }
        /// <summary>
        /// 移除Client
        /// </summary>
        /// <param name="key"></param>
        public void RemoveClient(string key)
        {
            if (ditClient.TryRemove(key, out var client))
            {
                client.Dispose();
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

        #endregion
    }
}
