using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB28181.Enums
{
    /// <summary>
    /// ON:上线,OFF:离线,VLOST:视频丢失,DEFECT:故障,ADD:增加,DEL:删除,UPDATE:更新(必选)
    /// </summary>
    public enum EventType : byte
    {
        /// <summary>
        /// 上线
        /// </summary>
        ON,
        /// <summary>
        /// 离线
        /// </summary>
        OFF,
        /// <summary>
        /// 视频丢失
        /// </summary>
        VLOST,
        /// <summary>
        /// 故障
        /// </summary>
        DEFECT,
        /// <summary>
        /// 增加
        /// </summary>
        ADD,
        /// <summary>
        /// 删除
        /// </summary>
        DEL,
        /// <summary>
        /// 更新
        /// </summary>
        UPDATE
    }
}
