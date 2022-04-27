using JX;
using System;
using System.Collections.Generic;
using System.Text;

namespace SipServer.Models
{
    public class VideoOrderAck
    {
        /// <summary>
        /// 返回值状态:0(初始)，1（成功）,2（设备不在线），3（失败），4（等待回应超时），5（等待回应中），6（作废）
        /// </summary>
        public int Status { get; set; }
        public JTVideoListInfo VideoList { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrMessage { get; set; }
    }
}
