
namespace GB28181.Enums
{
    /// <summary>
    /// 命令类型
    /// </summary>
    public enum CommandType
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown,
        /// <summary>
        /// 警告通知
        /// </summary>
        Alarm,
        /// <summary>
        /// 语音广播
        /// </summary>
        Broadcast,
        /// <summary>
        /// 设备目录
        /// </summary>
        Catalog,
        /// <summary>
        /// 设备配置
        /// </summary>
        ConfigDownload,
        /// <summary>
        /// 设备配置
        /// </summary>
        DeviceConfig,
        /// <summary>
        /// 设备控制
        /// </summary>
        DeviceControl,
        /// <summary>
        /// 设备信息
        /// </summary>
        DeviceInfo,
        /// <summary>
        /// 设备状态
        /// </summary>
        DeviceStatus,
        /// <summary>
        /// 媒体结束通知
        /// </summary>
        MediaStatus,
        /// <summary>
        /// 移动设备位置数据
        /// </summary>
        MobilePosition,
        /// <summary>
        /// 心跳
        /// </summary>
        Keepalive,
        /// <summary>
        /// 预置位查询
        /// </summary>
        PresetQuery,
        /// <summary>
        /// 文件检索
        /// </summary>
        RecordInfo,
    }
}
