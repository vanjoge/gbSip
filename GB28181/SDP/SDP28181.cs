using System;
using System.Collections.Generic;
using System.Text;

namespace GB28181
{
    public class SDP28181
    {
        public enum SType
        {
            Play,
            Playback,
            Talk,
            Download,
        }
        public enum RtpType
        {
            TCP,
            UDP,
        }
        public enum MediaStreamStatus
        {
            sendrecv = 0,   // The offerer is prepared to send and receive packets.
            sendonly = 1,   // The offerer only wishes to send RTP packets. They will probably ignore any received.
            recvonly = 2,   // The offerer only wishes to receive RTP packets. They will not send.
            inactive = 3    // The offerer is not ready to send or receive packets.
        }
        public string owner;
        public string ip;
        public int rtpPort;
        public bool onlyAudio;
        public string ssrc;
        public RtpType rtpType;
        public SType sType = SType.Play;
        public long tStart = 0;
        public long tEnd = 0;
        public string u;
        public MediaStreamStatus streamStatus = MediaStreamStatus.recvonly;
        string GetRTP_AVP()
        {
            if (rtpType == RtpType.UDP)
                return "RTP/AVP";
            else
                return "TCP/RTP/AVP";
        }

        string GetU()
        {
            if (u != null)
            {
                return $"\r\nu={u}:0";
            }
            return null;
        }
        string GetMA()
        {
            string m, a;
            if (onlyAudio)
            {
                m = $"m=audio {rtpPort} {GetRTP_AVP()} 8\r\n";
                a = @$"a={streamStatus}
a=rtpmap:8 PCMA/8000";
            }
            else
            {
                m = $"m=video {rtpPort} {GetRTP_AVP()} 96\r\n";
                a = @$"a={streamStatus}
a=rtpmap:96 PS/90000";
            }
            if (rtpType != 0)
            {
                a += @"
a=setup:passive
a=connection:new";
            }
            a += $"\r\na=ssrc:{ssrc.TrimStart('0')}";
            return m + a;
        }
        string GetF()
        {
            if (onlyAudio)
            {
                return "\r\nf=v/////a/1/8/1";
            }
            return null;
        }
        public string GetSdp()
        {
            return @$"v=0
o={owner} 0 0 IN IP4 {ip}
s={sType}{GetU()}
c=IN IP4 {ip}
t={tStart} {tEnd}
{GetMA()}
y={ssrc}{GetF()}
";

        }
    }
}
