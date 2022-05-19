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
        /// <summary>
        /// 发起录像查询缓存内容
        /// </summary>
        ConcurrentDictionary<int, QueryRecordInfo> ditQueryRecordInfo = new ConcurrentDictionary<int, QueryRecordInfo>();

        private Task<string> ReportRecordInfo(string OrderID, RecordInfo recordInfo)
        {
            var headers = new DictionaryEx<string, string>();
            headers.Add("Content-Type", "application/json");
            return HttpHelperByHttpClient.HttpRequestHtml($"{this.sipServer.Settings.RTVSAPI}api/GB/RecordInfo?OrderID={OrderID}", true, System.Threading.CancellationToken.None, headers: headers, data: recordInfo.ToJson());
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