using System;
using System.Collections.Generic;

namespace SipServer.DBModel
{
    /// <summary>
    /// 上级绑定分组
    /// </summary>
    public partial class TSuperiorGroup
    {
        /// <summary>
        /// 上级ID
        /// </summary>
        public string SuperiorId { get; set; }
        /// <summary>
        /// 分组ID
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// 包含下级分组
        /// </summary>
        public bool HasChild { get; set; }
    }
}
