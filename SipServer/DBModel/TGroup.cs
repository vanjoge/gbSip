using System;
using System.Collections.Generic;

namespace SipServer.DBModel
{
    /// <summary>
    /// 系统目录分组
    /// </summary>
    public partial class TGroup
    {
        /// <summary>
        /// 分组ID(215业务分组 216虚拟组织)
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// 上级ID
        /// </summary>
        public string ParentId { get; set; }
        /// <summary>
        /// 分组名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 查询路径 /分割
        /// </summary>
        public string Path { get; set; }
    }
}
