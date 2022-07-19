using GB28181;
using GB28181.Client;
using GB28181.XML;
using SIPSorcery.SIP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SipServer.Cascade
{
    public class CascadeClient : GB28181SipClient
    {
        public string Key { get; protected set; }
        public CascadeClient(string Key, string server, string server_id, DeviceInfo deviceInfo, IEnumerable<Catalog.Item> deviceList, string password = "123456", int expiry = 7200, string UserAgent = "rtvs v1", bool EnableTraceLogs = false, double heartSec = 60, double timeOutSec = 300) : base(server, server_id, deviceInfo, deviceList, password, expiry, UserAgent, EnableTraceLogs, heartSec, timeOutSec)
        {
            this.Key = Key;
        }

        protected override Task<bool> On_ACK(string fromTag, SIPRequest sipRequest)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> On_BYE(string fromTag, SIPRequest sipRequest)
        {
            throw new NotImplementedException();
        }

        protected override Task<SDP28181> On_INVITE(string fromTag, SDP28181 sdp, SIPRequest sipRequest)
        {
            throw new NotImplementedException();
        }

        protected override Task<RecordInfo> On_RECORDINFO(RecordInfoQuery res, SIPRequest sipRequest)
        {
            throw new NotImplementedException();
        }
    }
}
