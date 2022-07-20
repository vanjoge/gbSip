using GB28181;
using GB28181.Enums;
using GB28181.XML;
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
        class QueryRecordInfo
        {
            public string OrderID;
            public RecordInfo Info;
            public int SNOld;
        }
        class BroadcastInfo
        {
            public string Channel;
            public string InviteID;
        }
        /// <summary>
        /// 发起录像查询缓存内容
        /// </summary>
        ConcurrentDictionary<int, QueryRecordInfo> ditQueryRecordInfo = new ConcurrentDictionary<int, QueryRecordInfo>();

        ConcurrentDictionary<string, BroadcastInfo> ditBroadcast = new ConcurrentDictionary<string, BroadcastInfo>();
        private Task<string> ReportRecordInfo(string OrderID, RecordInfo recordInfo)
        {
            var headers = new DictionaryEx<string, string>();
            headers.Add("Content-Type", "application/json");
            return HttpHelperByHttpClient.HttpRequestHtml($"{this.sipServer.Settings.RTVSAPI}api/GB/RecordInfo?OrderID={OrderID}", true, System.Threading.CancellationToken.None, headers: headers, data: recordInfo.ToJson());
        }
        private Task<string> AnsBroadcastSDP(string SourceID, BroadcastInfo info, string sdp)
        {
            return HttpHelperByHttpClient.HttpRequestHtml($"{this.sipServer.Settings.RTVSAPI}api/GB/AnsBroadcastSDP?DeviceID={DeviceID}&Channel={info.Channel}&SourceID={SourceID}&InviteID={info.InviteID}&SDP={sdp.EncryptToBase64()}", false, System.Threading.CancellationToken.None);
        }

        async Task AckProcess(SIPEndPoint localSipEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
        {
            //var SourceID = sipRequest.Header.To.ToURI.User;
            var SourceID = sipRequest.Header.From.FromURI.User;
            if (ditBroadcast.TryRemove(SourceID, out var info) && info.InviteID == sipRequest.Header.To.ToTag)
            {
                await HttpHelperByHttpClient.HttpRequestHtml($"{this.sipServer.Settings.RTVSAPI}api/GB/BroadcastAck?DeviceID={DeviceID}&Channel={info.Channel}&SourceID={SourceID}&InviteID={info.InviteID}", false, System.Threading.CancellationToken.None);
            }
        }
        async Task ByeProcess(SIPEndPoint localSipEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
        {
            await SendOkMessage(sipRequest);
            var InviteID = sipRequest.Header.To.ToTag;
            if (sipServer.RemoveTag(InviteID))
            {
                await HttpHelperByHttpClient.HttpRequestHtml($"{this.sipServer.Settings.RTVSAPI}api/GB/BroadcastBye?DeviceID={DeviceID}&InviteID={InviteID}", false, System.Threading.CancellationToken.None);
            }
        }
        async Task InviteProcess(SIPEndPoint localSipEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
        {
            var SourceID = sipRequest.Header.From.FromURI.User;
            if (ditBroadcast.TryGetValue(SourceID, out var info))
            {
                await SendResponseAsync(GetSIPResponse(sipRequest, SIPResponseStatusCodesEnum.Trying));
                //调用接口返回SDP
                var sdp = await AnsBroadcastSDP(SourceID, info, sipRequest.Body);
                if (string.IsNullOrWhiteSpace(sdp))
                {
                    ditBroadcast.TryRemove(SourceID, out var info2);
                    var res = GetSIPResponse(sipRequest, SIPResponseStatusCodesEnum.NotAcceptableHere);
                    res.Header.To.ToTag = info.InviteID;
                    await SendResponseAsync(res);
                }
                else
                {
                    var res = GetSIPResponse(sipRequest);
                    res.Header.Contact = new List<SIPContactHeader> { new SIPContactHeader(sipRequest.Header.To.ToUserField) };
                    res.Header.To.ToTag = info.InviteID;
                    res.Header.ContentType = Constant.Application_SDP;
                    res.Body = sdp;


                    sipServer.SetTag(info.InviteID, new FromTagItem { Client = this, From = res.Header.From, To = res.Header.To });
                    await SendResponseAsync(res);
                }
            }
            else
            {
                await SendResponseAsync(GetSIPResponse(sipRequest, SIPResponseStatusCodesEnum.NotFound));
            }
        }
        private Task<string> AnsRTVSGetRecordInfo(RecordInfo recordInfo)
        {
            if (ditQueryRecordInfo.TryGetValue(recordInfo.SN, out var query))
            {
                if (recordInfo.SumNum == 0)
                {
                    ditQueryRecordInfo.TryRemove(recordInfo.SN, out query);
                    return ReportRecordInfo(query.OrderID, recordInfo);
                }
                else
                {
                    if (query.Info == null)
                    {
                        query.Info = recordInfo;
                    }
                    else if (recordInfo.SumNum > recordInfo.RecordList.Count)
                    {
                        query.Info.RecordList.AddRange(recordInfo.RecordList);
                    }

                    if (recordInfo.SumNum <= query.Info.RecordList.Count)
                    {
                        ditQueryRecordInfo.TryRemove(recordInfo.SN, out query);
                        return ReportRecordInfo(query.OrderID, query.Info);
                    }
                }
            }
            return Task.FromResult<string>(null);
        }
    }
}