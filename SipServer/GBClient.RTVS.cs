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
        class BroadcastInfo
        {
            public string Channel;
            public string InviteID;
        }

        ConcurrentDictionary<string, BroadcastInfo> ditBroadcast = new ConcurrentDictionary<string, BroadcastInfo>();

        public Task<int> Send_GetRecordInfo_RTVS(string OrderID, RecordInfoQuery query)
        {
            return Send_GetRecordInfo(query, (p) =>
            {
                return ReportRecordInfo(OrderID, p);
            });
        }
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
        private Task<string> AnsSDP(string Channel, string InviteID, string sdp)
        {
            return HttpHelperByHttpClient.HttpRequestHtml($"{this.sipServer.Settings.RTVSAPI}api/GB/AnsSDP?DeviceID={DeviceID}&Channel={Channel}&InviteID={InviteID}&SDP={sdp.EncryptToBase64()}", false, System.Threading.CancellationToken.None);
        }

        async Task AckProcess(SIPEndPoint localSipEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
        {
            var SourceID = GetSourceID(sipRequest);
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
            var SourceID = GetSourceID(sipRequest);
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


        string GetSourceID(SIPRequest sipRequest)
        {
            if (sipRequest.Header.To.ToURI.User == ServerID)
            {
                return sipRequest.Header.From.FromURI.User;
            }
            else
            {
                return sipRequest.Header.To.ToURI.User;
            }
        }
    }
}