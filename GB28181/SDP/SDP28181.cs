﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using SQ.Base;

namespace GB28181
{
    public class SDP28181
    {
        static System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("([^=]+)=(.+)", System.Text.RegularExpressions.RegexOptions.Compiled);
        static System.Text.RegularExpressions.Regex regMap = new System.Text.RegularExpressions.Regex(@"rtpmap:(\d+) ([^/]+)/(\d+)");
        /// <summary>
        /// 
        /// </summary>
        public enum MediaType
        {
            /// <summary>
            /// 
            /// </summary>
            video,
            /// <summary>
            /// 
            /// </summary>
            audio,
        }
        /// <summary>
        /// 
        /// </summary>
        public enum PlayType
        {
            /// <summary>
            /// 未知
            /// </summary>
            Unknown,
            /// <summary>
            /// 实时
            /// </summary>
            Play,
            /// <summary>
            /// 历史
            /// </summary>
            Playback,
            /// <summary>
            /// 对讲
            /// </summary>
            Talk,
            /// <summary>
            /// 下载
            /// </summary>
            Download,
        }
        /// <summary>
        /// RTP模式
        /// </summary>
        public enum RTPNetType
        {
            /// <summary>
            /// TCP
            /// </summary>
            TCP,
            /// <summary>
            /// UDP
            /// </summary>
            UDP,
        }
        /// <summary>
        /// 
        /// </summary>
        public enum MediaStreamStatus
        {
            /// <summary>
            /// The offerer is prepared to send and receive packets.
            /// </summary>
            sendrecv = 0,
            /// <summary>
            /// The offerer only wishes to send RTP packets. They will probably ignore any received.
            /// </summary>
            sendonly = 1,
            /// <summary>
            /// The offerer only wishes to receive RTP packets. They will not send.
            /// </summary>
            recvonly = 2,
            /// <summary>
            /// The offerer is not ready to send or receive packets.
            /// </summary>
            inactive = 3
        }

        public class RTPMap
        {
            public int ID;
            public string Name;
            public int ClockRate;
        }
        /// <summary>
        /// o=
        /// </summary>
        public string Owner;
        /// <summary>
        /// 本机SIP信令交互地址
        /// </summary>
        public string LocalIp;
        /// <summary>
        /// 流媒体地址
        /// </summary>
        public string RtpIp;
        /// <summary>
        /// RTP流端口
        /// </summary>
        public int RtpPort;
        /// <summary>
        /// 
        /// </summary>
        public MediaType Media;
        /// <summary>
        /// SSRC
        /// </summary>
        public string SSRC;
        /// <summary>
        /// RTP连接类型
        /// </summary>
        public RTPNetType NetType;
        /// <summary>
        /// 播放类型
        /// </summary>
        public PlayType SType = PlayType.Play;
        /// <summary>
        /// 开始时间 实时为0 历史为UTC时间戳
        /// </summary>
        public long TStart = 0;
        /// <summary>
        /// 结束时间 实时为0 历史为UTC时间戳
        /// </summary>
        public long TEnd = 0;
        /// <summary>
        /// u=
        /// </summary>
        public string u;
        /// <summary>
        /// 
        /// </summary>
        public MediaStreamStatus streamStatus = MediaStreamStatus.recvonly;
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<int, RTPMap> RtpMaps = new Dictionary<int, RTPMap>();
        /// <summary>
        /// f=
        /// </summary>
        public string f;
        /// <summary>
        /// 下载倍速
        /// </summary>
        public int Downloadspeed = 4;
        /// <summary>
        /// 加上a=ssrc标准输出
        /// </summary>
        public bool ASSRC = true;
        /// <summary>
        /// 原始SDP文本
        /// </summary>
        public string RawStr { get; protected set; }
        string GetRTP_AVP()
        {
            if (NetType == RTPNetType.UDP)
                return "RTP/AVP";
            else
                return "TCP/RTP/AVP";
        }

        string GetMA()
        {
            string m, a = $"a={streamStatus}";
            m = $"m={Media} {RtpPort} {GetRTP_AVP()}";
            foreach (var keyValue in RtpMaps)
            {
                m += " " + keyValue.Key;
                a += $"\r\na=rtpmap:{keyValue.Key} {keyValue.Value.Name}/{keyValue.Value.ClockRate}";
            }
            if (NetType == RTPNetType.TCP)
            {
                a += "\r\na=setup:passive\r\na=connection:new";
            }
            if (SType == PlayType.Download && Downloadspeed > 0)
            {
                a += $"\r\na=downloadspeed:{Downloadspeed}";
            }
            if (ASSRC)
                a += $"\r\na=ssrc:{SSRC.TrimStart('0')}";
            return m + "\r\n" + a;
        }
        string GetF()
        {
            if (f != null)
            {
                return $"\r\nf={f}";
            }
            return null;
        }
        /// <summary>
        /// 获取SDP字符串
        /// </summary>
        /// <returns></returns>
        public string GetSdpStr()
        {
            return @$"v=0
o={Owner} 0 0 IN IP4 {LocalIp}
s={SType}{(u != null ? $"\r\nu={u}" : "")}
c=IN IP4 {RtpIp}
t={TStart} {TEnd}
{GetMA()}
y={SSRC.StrFixLen(10)}{GetF()}
";

        }
        /// <summary>
        /// 解析SDP文本并填充
        /// </summary>
        /// <param name="sdp"></param>
        public void Fill(string sdp)
        {
            this.RawStr = sdp;
            foreach (Match mth in reg.Matches(sdp))
            {
                var tstr = mth.Groups[2].Value.Trim();
                switch (mth.Groups[1].Value.Trim())
                {
                    case "o":
                        {
                            var arr = tstr.Split(" ");
                            if (arr.Length >= 6)
                            {
                                Owner = arr[0];
                                LocalIp = arr[5];
                            }
                        }
                        break;
                    case "s":
                        {
                            if (Enum.TryParse<PlayType>(tstr, true, out var res))
                            {
                                SType = res;
                            }
                            else
                            {
                                SType = PlayType.Unknown;
                            }
                        }
                        break;
                    case "u":
                        {
                            u = tstr;
                        }
                        break;
                    case "f":
                        {
                            f = tstr;
                        }
                        break;
                    case "c":
                        {
                            var arr = tstr.Split(" ");
                            if (arr.Length >= 3)
                            {
                                RtpIp = arr[2];
                            }
                        }
                        break;
                    case "t":
                        {
                            var arr = tstr.Split(" ");
                            if (arr.Length >= 2)
                            {
                                if (long.TryParse(arr[0], out var s) && long.TryParse(arr[1], out var e))
                                {
                                    TStart = s;
                                    TEnd = e;
                                }
                            }
                        }
                        break;
                    case "m":
                        {
                            var arr = tstr.Split(" ");
                            if (arr.Length >= 4)
                            {
                                if ("audio".IgnoreEquals(arr[0]))
                                {
                                    Media = MediaType.audio;
                                }
                                else if ("video".IgnoreEquals(arr[0]))
                                {
                                    Media = MediaType.video;
                                }
                                else
                                {
                                    break;
                                }
                                if (int.TryParse(arr[1], out var p))
                                {
                                    RtpPort = p;
                                }
                                if ("RTP/AVP".IgnoreEquals(arr[2]))
                                {
                                    NetType = RTPNetType.UDP;
                                }
                                else if ("TCP/RTP/AVP".IgnoreEquals(arr[2]))
                                {
                                    NetType = RTPNetType.TCP;
                                }

                                for (int i = 3; i < arr.Length; i++)
                                {
                                    if (int.TryParse(arr[i], out p))
                                    {
                                        RtpMaps.Add(p, null);
                                    }
                                }
                            }
                        }
                        break;
                    case "y":
                        {
                            SSRC = tstr;
                        }
                        break;
                    case "a":
                        {
                            var mth2 = regMap.Match(tstr);

                            if (mth2.Success)
                            {
                                var id = Convert.ToInt32(mth2.Groups[1].Value);
                                if (RtpMaps.ContainsKey(id))
                                {
                                    RtpMaps[id] = new RTPMap
                                    {
                                        ID = id,
                                        Name = mth2.Groups[2].Value,
                                        ClockRate = Convert.ToInt32(mth2.Groups[3].Value),
                                    };
                                }
                            }
                            else if (Enum.TryParse<MediaStreamStatus>(tstr, true, out var mss))
                            {
                                streamStatus = mss;
                            }
                            else
                            {
                                var idx = tstr.IndexOf("downloadspeed:");
                                if (idx >= 0 && int.TryParse(tstr.Substring(idx + 14), out Downloadspeed))
                                {
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 获取应答SDP
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="localIp"></param>
        /// <param name="rtpIp"></param>
        /// <param name="rtpPort"></param>
        /// <returns></returns>
        public SDP28181 AnsSdp(string owner, string localIp, string rtpIp, int rtpPort)
        {
            var ret = new SDP28181
            {
                ASSRC = this.ASSRC,
                f = this.f,
                Media = this.Media,
                NetType = this.NetType,
                RtpMaps = this.RtpMaps,
                SSRC = this.SSRC,
                SType = this.SType,
                TEnd = this.TEnd,
                TStart = this.TStart,
                u = this.u,
                LocalIp = localIp,
                Owner = owner,
                RtpIp = rtpIp,
                RtpPort = rtpPort,

            };
            switch (this.streamStatus)
            {
                case MediaStreamStatus.sendrecv:
                    ret.streamStatus = MediaStreamStatus.sendrecv;
                    break;
                case MediaStreamStatus.sendonly:
                    ret.streamStatus = MediaStreamStatus.recvonly;
                    break;
                case MediaStreamStatus.recvonly:
                    ret.streamStatus = MediaStreamStatus.sendonly;
                    break;
                case MediaStreamStatus.inactive:
                    ret.streamStatus = MediaStreamStatus.inactive;
                    break;
                default:
                    break;
            }
            if (ret.SSRC == null)
            {
                ret.SSRC = "3333333333";
            }
            return ret;
        }
        /// <summary>
        /// 用SDP文本初始化
        /// </summary>
        /// <param name="sdp"></param>
        /// <returns></returns>
        public static SDP28181 NewByStr(string sdp)
        {
            SDP28181 model = new SDP28181();
            model.Fill(sdp);
            return model;
        }
    }
    /// <summary>
    /// PS流的SDP
    /// </summary>
    public class SDP28181PS : SDP28181
    {
        public SDP28181PS()
        {
            this.Media = MediaType.video;
            this.RtpMaps.Add(96, new RTPMap { ClockRate = 90000, ID = 96, Name = "PS" });
        }
    }
    /// <summary>
    /// PCMA的SDP
    /// </summary>
    public class SDP28181PCMA : SDP28181
    {
        public SDP28181PCMA()
        {
            this.Media = MediaType.audio;
            this.RtpMaps.Add(8, new RTPMap { ClockRate = 8000, ID = 8, Name = "PCMA" });
            this.f = "v/////a/1/8/1";
        }
    }
}
