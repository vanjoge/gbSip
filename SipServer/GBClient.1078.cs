using GB28181;
using GB28181.Enums;
using GB28181.XML;
using JX;
using SipServer.Models;
using SIPSorcery.SIP;
using SQ.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SipServer
{
    public partial class GBClient
    {
        public const string VideoControlSuccess = "1";
        public const string VideoControlFail = "-1";
        public const string VideoControlOffline = "0";
        class InvCache
        {
            public DateTime dtStart;
            public DateTime dtEnd;
            public string fromTag;
        }
        /// <summary>
        /// 发起视频缓存内容
        /// </summary>
        ConcurrentDictionary<string, InvCache> ditInvTag = new ConcurrentDictionary<string, InvCache>();
        class QueryRecordInfo
        {
            public string OrderId;
            public List<RecordInfo.Item> lst = new List<RecordInfo.Item>();
            public ushort SN808;
        }
        /// <summary>
        /// 发起录像查询缓存内容
        /// </summary>
        ConcurrentDictionary<int, QueryRecordInfo> ditQueryRecordInfo = new ConcurrentDictionary<int, QueryRecordInfo>();

        /// <summary>
        /// 处理入口
        /// </summary>
        /// <param name="isSuperiorPlatformSend"></param>
        /// <param name="head"></param>
        /// <param name="bts"></param>
        /// <returns></returns>
        internal async Task<string> HandleJT1078(bool isSuperiorPlatformSend, JTHeader head, byte[] bts)
        {
            switch (head.MsgId)
            {
                case 0x9101:
                    return await JX9101(head, bts);
                case 0x9102://音视频实时传输控制
                    return await JX9102(head, bts);
                case 0x9201://平台下发远程录像回放请求
                    return await JX9201(head, bts);

                case 0x9202://平台下发远程录像回放控制
                    return await JX9202(head, bts);
                case 0x9205://查询回放文件
                    return await JX9205(head, bts);
                default:
                    break;
            }

            return VideoControlFail;
        }


        bool GetRtpTypeAndPort(int TcpPort, int UdpPort, out int port, out SDP28181.RtpType rtpType)
        {
            if (TcpPort > 0)
            {
                port = TcpPort;
                rtpType = SDP28181.RtpType.TCP;
                return true;
            }
            else if (UdpPort > 0)
            {
                port = UdpPort;
                rtpType = SDP28181.RtpType.UDP;
                return true;
            }
            port = 0;
            rtpType = SDP28181.RtpType.TCP;
            return false;
        }

        async Task<string> Send_GetRecordInfo2(JTHeader head, string chid, string qtype, DateTime dtStart, DateTime dtEnd)
        {
            var sn = await Send_GetRecordInfo(chid, qtype, dtStart, dtEnd);

            var orderid = Guid.NewGuid().ToString();
            ditQueryRecordInfo[sn] = new QueryRecordInfo
            {
                OrderId = orderid,
                SN808 = head.SerialNumber
            };
            return orderid;
        }


        /// <summary>
        /// 处理实时视频
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bts"></param>
        /// <returns></returns>
        async Task<string> JX9101(JTHeader head, byte[] bts)
        {
            var req = JTRealVideoTransferRequest.NewEntity(bts, head.HeadLen);

            var key = head.Sim + "_" + req.Channel;
            bool isRA;
            if (req.DataType == 0 || req.DataType == 1 || req.DataType == 3)
            {
                key += "_R";
                isRA = false;
            }
            else
            {
                key += "_RA";
                isRA = true;
            }
            if (req.Channel <= deviceList.Count && deviceList.TryGetCHID(req.Channel, out var chid) && GetRtpTypeAndPort(req.TcpPort, req.UdpPort, out var port, out var rtpType))
            {
                if (ditInvTag.TryGetValue(key, out var item))
                {
                    await Send_Bye(item.fromTag);
                }
                var ssrc = sipServer.GetNewSSRC(chid, false);
                var fromTag = "INVITE_" + CallProperties.CreateNewTag();

                ditInvTag[key] = new InvCache { fromTag = fromTag };

                if (req.DataType < 2)
                {
                    await Send_INVITE(req.IPAddress, port, chid, ssrc, fromTag, rtpType, GB28181.SDP28181.SType.Play);
                    return VideoControlSuccess;
                }
                else if (req.DataType < 5)
                {
                    //if (deviceInfo.Manufacturer == "Hikvision")
                    //{
                    //    await Send_INVITE(req.IPAddress, port, chid, ssrc, fromTag, rtpType, GB28181.SDP28181.SType.Talk,false, GB28181.SDP28181.MediaStreamStatus.sendrecv);
                    //    //await Send_Broadcast(DeviceId, chid);
                    //}
                    //else
                    await Send_INVITE(req.IPAddress, port, chid, ssrc, fromTag, rtpType, GB28181.SDP28181.SType.Talk, true, GB28181.SDP28181.MediaStreamStatus.sendrecv);
                    return VideoControlSuccess;
                }
            }
            return VideoControlFail;
        }

        async Task FindRRA(string sim, byte channel, bool r, bool ra, Func<string, InvCache, Task> action)
        {
            if (r)
            {
                var key = sim + "_" + channel + "_R";
                if (ditInvTag.TryGetValue(key, out var item))
                {
                    await action(key, item);
                }
            }
            if (ra)
            {
                var key = sim + "_" + channel + "_RA";
                if (ditInvTag.TryGetValue(key, out var item))
                {
                    await action(key, item);
                }
            }
        }
        /// <summary>
        /// 处理实时控制
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bts"></param>
        /// <returns></returns>
        async Task<string> JX9102(JTHeader head, byte[] bts)
        {

            var rtc = JTRealVideoTransferControl.NewEntity(bts, head.HeadLen);
            if (rtc.ControlCommand == 0)
            {
                await FindRRA(head.Sim, rtc.Channel, true, true, async (key, item) =>
                {
                    await Send_Bye(item.fromTag);
                    ditInvTag.TryRemove(key, out var rm);
                });
            }
            else if (rtc.ControlCommand == 2)
            {
                await FindRRA(head.Sim, rtc.Channel, true, true, async (key, item) =>
                {
                    await Send_MANSRTSP(item.fromTag, new GB28181.MANSRTSP.MrtspRequest(new GB28181.MANSRTSP.MrtspRequest.Head
                    {
                        CSeq = GetCseq()
                    })
                    {
                        Method = SIPSorcery.Net.RTSPMethodsEnum.PAUSE,
                    });
                });
            }
            else if (rtc.ControlCommand == 3)
            {
                await FindRRA(head.Sim, rtc.Channel, true, true, async (key, item) =>
                {
                    await Send_MANSRTSP(item.fromTag, new GB28181.MANSRTSP.MrtspRequest(new GB28181.MANSRTSP.MrtspRequest.Head
                    {
                        CSeq = GetCseq()
                    })
                    {
                        Method = SIPSorcery.Net.RTSPMethodsEnum.PLAY,
                    });
                });
            }
            else if (rtc.ControlCommand == 4)
            {
                await FindRRA(head.Sim, rtc.Channel, false, true, async (key, item) =>
                {
                    await Send_Bye(item.fromTag);
                    ditInvTag.TryRemove(key, out var rm);
                });
            }
            return VideoControlSuccess;
        }
        /// <summary>
        /// 处理历史视频请求
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bts"></param>
        /// <returns></returns>
        async Task<string> JX9201(JTHeader head, byte[] bts)
        {
            var req = JTVideoPlaybackRequest.NewEntity(bts, head.HeadLen);
            if (req.Channel <= deviceList.Count)
            {
                if (req.Channel == 0)
                {
                    req.Channel = 1;
                }
                if (deviceList.TryGetCHID(req.Channel, out var chid) && GetRtpTypeAndPort(req.TcpPort, req.UdpPort, out var port, out var rtpType))
                {
                    if (req.MediaType == AudioVideoFlag.VideoOrAudioVideo)
                    {
                        req.MediaType = AudioVideoFlag.AudioVideo;
                    }
                    if (req.StorageType == MemoryType.AllMemory)
                    {
                        req.StorageType = MemoryType.MainMemory;
                    }
                    var key = head.Sim + "_" + req.Channel + "_H" + req.StartTime.Ticks + "_" + req.EndTime;

                    var dtEnd = req.EndTime.UNIXtoDateTime();
                    var orderid = await Send_GetRecordInfo2(head, chid, "all", req.StartTime, dtEnd);

                    if (ditInvTag.TryGetValue(key, out var item))
                    {
                        await Send_Bye(item.fromTag);
                    }
                    var ssrc = sipServer.GetNewSSRC(chid, true);
                    var fromTag = "INVITEBack_" + CallProperties.CreateNewTag();

                    ditInvTag[key] = new InvCache { fromTag = fromTag, dtStart = req.StartTime, dtEnd = dtEnd };


                    await Send_INVITE_BACK(req.IPAddress, port, chid, ssrc, fromTag, req.StartTime.DateTimeToUNIX_long(), req.EndTime, rtpType);
                    return orderid;
                }
            }
            return VideoControlFail;
        }
        /// <summary>
        /// 处理历史视频控制
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bts"></param>
        /// <returns></returns>
        async Task<string> JX9202(JTHeader head, byte[] bts)
        {
            var reqpb = JTVideoPlaybackControl.NewEntity(bts, head.HeadLen);

            foreach (var key in ditInvTag.Keys.ToArray())
            {
                if (key.StartsWith(head.Sim + "_" + reqpb.Channel + "_H"))
                {
                    if (ditInvTag.TryGetValue(key, out var item))
                    {
                        switch (reqpb.PlaybackControl)
                        {
                            case 0://开始
                                await Send_MANSRTSP(item.fromTag, new GB28181.MANSRTSP.MrtspRequest(new GB28181.MANSRTSP.MrtspRequest.Head
                                {
                                    CSeq = GetCseq()
                                })
                                {
                                    Method = SIPSorcery.Net.RTSPMethodsEnum.PLAY,
                                });
                                return VideoControlSuccess;
                            case 1://暂停
                                await Send_MANSRTSP(item.fromTag, new GB28181.MANSRTSP.MrtspRequest(new GB28181.MANSRTSP.MrtspRequest.Head
                                {
                                    CSeq = GetCseq()
                                })
                                {
                                    Method = SIPSorcery.Net.RTSPMethodsEnum.PAUSE,
                                });
                                return VideoControlSuccess;
                            case 2://关闭
                                await Send_Bye(item.fromTag);
                                ditInvTag.TryRemove(key, out item);
                                return VideoControlSuccess;
                            case 3://快进
                            case 4://快进
                                double Scale;
                                switch (reqpb.Multiple)
                                {
                                    case 1:
                                        Scale = 1;
                                        break;
                                    case 2:
                                        Scale = 2;
                                        break;
                                    case 3:
                                        Scale = 4;
                                        break;
                                    case 4:
                                        Scale = 8;
                                        break;
                                    case 5:
                                        Scale = 16;
                                        break;
                                    default:
                                        return VideoControlFail;
                                }
                                if (reqpb.PlaybackControl == 4)
                                {
                                    Scale = -Scale;
                                }
                                await Send_MANSRTSP(item.fromTag, new GB28181.MANSRTSP.MrtspRequest(new GB28181.MANSRTSP.MrtspRequest.Head
                                {
                                    CSeq = GetCseq(),
                                    Scale = Scale
                                })
                                {
                                    Method = SIPSorcery.Net.RTSPMethodsEnum.PLAY,
                                });
                                return VideoControlSuccess;
                            case 5:
                                if (item.dtStart <= reqpb.DragPlaybackPosition && item.dtEnd >= reqpb.DragPlaybackPosition)
                                {
                                    var range = new GB28181.MANSRTSP.MrtspRequest.Range();
                                    range.Start = (reqpb.DragPlaybackPosition - item.dtStart).TotalSeconds;
                                    await Send_MANSRTSP(item.fromTag, new GB28181.MANSRTSP.MrtspRequest(new GB28181.MANSRTSP.MrtspRequest.Head
                                    {
                                        CSeq = GetCseq(),
                                        Range = range
                                    })
                                    {
                                        Method = SIPSorcery.Net.RTSPMethodsEnum.PLAY,
                                    });
                                    return VideoControlSuccess;
                                }
                                break;
                        }
                    }
                }
            }
            return VideoControlFail;
        }
        /// <summary>
        /// 处理历史视频查询
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bts"></param>
        /// <returns></returns>
        async Task<string> JX9205(JTHeader head, byte[] bts)
        {
            JTQueryVideoFileList qvf = JTQueryVideoFileList.NewEntity(bts, head.HeadLen);

            string chid;
            string qtype = "all";
            if (qvf.Channel == 0)
            {
                chid = this.DeviceId;
            }
            else
            {
                if (!deviceList.TryGetCHID(qvf.Channel, out chid))
                {
                    return VideoControlFail;
                }
            }
            if (qvf.Alarm != 0)
            {
                qtype = "alarm";
            }
            DateTime dtStart = qvf.StartTime.UNIXtoDateTime();
            DateTime dtEnd = qvf.EndTime.UNIXtoDateTime();

            return await Send_GetRecordInfo2(head, chid, qtype, dtStart, dtEnd);
        }
    }
}