using System;
using System.Collections.Generic;
using System.Text;

namespace JTServer.Model.RTVS
{
    /// <summary>
    /// RTP推送任务
    /// </summary>
    public class SendRTPTask : RETModel
    {
        /// <summary>
        /// RTP推送任务唯一id，用此id区分多次任务
        /// </summary>
        public string TaskID { get; set; }
        /// <summary>
        /// 本地IP
        /// </summary>
        public string LocIP { get; set; }
        /// <summary>
        /// 本地端口
        /// </summary>
        public int LocPort { get; set; }
    }
}
