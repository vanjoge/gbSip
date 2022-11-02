using GB28181.XML;
using SipServer.Cascade;
using SipServer.DBModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipServer.Models
{
    public class SuperiorInfoEx : TSuperiorInfo
    {
        private CascadeClient Client;
        public void SetClient(CascadeClient Client)
        {
            this.Client = Client;
        }
        public SuperiorInfoEx(TSuperiorInfo item)
        {
            ClientId = item.ClientId;
            ClientName = item.ClientName;
            Enable = item.Enable;
            Expiry = item.Expiry;
            HeartSec = item.HeartSec;
            HeartTimeoutTimes = item.HeartTimeoutTimes;
            Id = item.Id;
            Name = item.Name;
            RegSec = item.RegSec;
            Server = item.Server;
            ServerId = item.ServerId;
            ServerPort = item.ServerPort;
            Sippassword = item.Sippassword;
            Sipusername = item.Sipusername;
            UseTcp = item.UseTcp;
            ServerRealm = item.ServerRealm;
        }

        public string GetServerSipStr()
        {
            return GetServerSipStr(this);
        }
        public static string GetServerSipStr(TSuperiorInfo superior)
        {
            return $"sip:{superior.ServerId}@{superior.Server}:{superior.ServerPort}{(superior.UseTcp ? ";transport=tcp" : "")}";
        }
        public static void Check(TSuperiorInfo info)
        {
            if (info.ClientName == null) info.ClientName = "";
            if (info.Sippassword == null) info.Sippassword = "";
            if (info.Sipusername == null) info.Sipusername = "";
        }
    }
}
