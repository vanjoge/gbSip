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
    public class SuperiorInfoEx
    {
        public TSuperiorInfo superiorInfo;

        public CascadeClient Client;
        public string GetServerSipStr()
        {
            return GetServerSipStr(superiorInfo);
        }
        public static string GetServerSipStr(TSuperiorInfo superior)
        {
            return $"sip:{superior.ServerId}@{superior.Server}:{superior.ServerPort}{(superior.UseTcp ? ";transport=tcp" : "")}";
        }
    }
}
