using System;
using System.Collections.Generic;
using System.Text;

namespace JTServer.Model.RTVS
{
    /// <summary>
    /// 状态码定义
    /// </summary>
    public enum StateCode
    {
        /// <summary>
        /// 服务端内部错误
        /// </summary>
        InternalError = -1000,
        /// <summary>
        /// 未找到对应任务
        /// </summary>
        NotFoundTask = -3,
        /// <summary>
        /// 超出限制
        /// </summary>
        OverLimit = -2,
        /// <summary>
        /// 失败
        /// </summary>
        Fail = -1,
        /// <summary>
        /// 成功
        /// </summary>
        Success = 1,
    }
}
