using SIPSorcery.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace GB28181.MANSRTSP
{
    public class MrtspRequest
    {
        public string RTSPVersion = "RTSP/1.0";
        public RTSPMethodsEnum Method;
        public Head Header;

        public string Body;

        public string RawStr { get; protected set; }
        public MrtspRequest()
        {
            Header = new Head();
        }
        public MrtspRequest(Head header)
        {
            Header = header;
        }
        public MrtspRequest(string str)
        {
            Header = new Head();
            this.Fill(str);
        }

        public void Fill(string rawstr)
        {
            RawStr = rawstr;
            Method = RTSPMethodsEnum.UNKNOWN;
            var arr = rawstr.Split(Constant.CRLF + Constant.CRLF);
            if (arr[0] != null)
            {
                var arrh = arr[0].Split(Constant.CRLF);
                var statusLine = arrh[0];
                int firstSpacePosn = statusLine.IndexOf(" ");

                string method = statusLine.Substring(0, firstSpacePosn).Trim().ToUpper();
                Method = RTSPMethods.GetMethod(method);
                RTSPVersion = statusLine.Substring(firstSpacePosn).Trim();

                for (int i = 1; i < arrh.Length; i++)
                {
                    int colonPosn = arrh[i].IndexOf(":");
                    if (colonPosn > -1)
                    {
                        switch (arrh[i].Substring(0, colonPosn).Trim().ToUpper())
                        {
                            case "CSEQ":
                                Header.CSeq = Convert.ToInt32(arrh[i].Substring(colonPosn + 1).Trim());
                                break;
                            case "SCALE":
                                Header.Scale = Convert.ToDouble(arrh[i].Substring(colonPosn + 1).Trim());
                                break;
                            case "RANGE":
                                this.Header.Range = Range.NewByStr(arrh[i].Substring(colonPosn + 1).Trim());
                                break;
                            //case "PAUSETIME":
                            //    if (method == "PAUSE")
                            //    {
                            //        Method = MrtspMethodsEnum.PAUSE;
                            //    }
                            //    break;
                            default:
                                break;
                        }
                    }
                }

                //Body
                if (arr.Length > 1)
                {
                    Body = arr[1];
                }
                else
                {
                    Body = null;
                }

            }

        }
        public override string ToString()
        {
            string ret =
                this.Method + " " + RTSPVersion + Constant.CRLF
                + "CSeq: " + Header.CSeq + Constant.CRLF;
            switch (Method)
            {
                case RTSPMethodsEnum.PAUSE:
                    ret += "PauseTime: now" + Constant.CRLF;
                    break;
                case RTSPMethodsEnum.PLAY:
                    if (Header.Scale.HasValue)
                    {
                        ret += "Scale: " + Header.Scale.Value.ToString("F1") + Constant.CRLF;
                    }
                    else if (Header.Range == null)
                    {
                        ret += "Range: npt=now-" + Constant.CRLF;
                    }
                    if (Header.Range != null)
                    {
                        ret += "Range: npt=" + Header.Range + Constant.CRLF;
                    }
                    break;
                case RTSPMethodsEnum.TEARDOWN:
                    break;
            }

            return ret + Constant.CRLF + Body;
        }

        public class Head
        {
            public int CSeq = -1;
            /// <summary>
            /// Scale为1,正常播放;不等于1,为正常播放速率的倍数;负数为倒放 应支持 0.25、0.5、1、2、4
            /// </summary>
            public double? Scale;
            /// <summary>
            ///  播放录像起点的相对值,取值范围为0到播放录像的终点时间,参数以s为单位,
            ///  负值表示当前位置。如 Range头的值为0, 则表示从起点开始播放, Range头的值为100, 则表示从录像起点后
            ///  的100s处开始播放,Range头的取值为now 表示从当前位置开始播放。
            /// </summary>
            public Range Range;

        }
        static System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("npt=([^-]+)-(\\d+)?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        public class Range
        {

            /// <summary>
            /// 
            /// </summary>
            public bool StartIsNow
            {
                get
                {
                    return Start == double.MinValue;
                }
                set
                {
                    if (value)
                    {
                        Start = double.MinValue;
                    }
                }
            }
            /// <summary>
            ///  播放录像起点的相对值,取值范围为0到播放录像的终点时间,参数以s为单位,
            ///  不能为负值 如 Range头的值为0, 则表示从起点开始播放, Range头的值为100, 则表示从录像起点后
            ///  的100s处开始播放,Range头的取值为now 表示从当前位置开始播放。
            /// </summary>
            public double Start = double.MinValue;
            /// <summary>
            /// 
            /// </summary>
            public double? End;

            public static Range NewByStr(string str)
            {
                var mth = reg.Match(str);
                if (mth.Success)
                {
                    var rg = new Range();
                    if (mth.Groups[1].Value.ToLower() == "now")
                    {
                        rg.StartIsNow = true;
                    }
                    else
                    {
                        rg.Start = Convert.ToDouble(mth.Groups[1].Value);
                        if (mth.Groups[2].Success)
                        {
                            rg.End = Convert.ToDouble(mth.Groups[2].Value);
                        }
                    }
                    return rg;
                }
                return null;
            }
            public override string ToString()
            {
                if (StartIsNow)
                {
                    return "now-" + End?.ToString();
                }
                else
                {
                    return Start + "-" + End?.ToString();
                }
            }
        }
    }
}
