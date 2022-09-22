declare namespace API {
  type TDeviceInfo = {
    DeviceId: string;
    DeviceName: string;
    Manufacturer: string;
    Model: string;
    Firmware: string;
    Channel: number;
    Reported: boolean;
    CatalogChannel: number;
    GetCatalogTime: Date;
    Online: boolean;
    OnlineTime: Date;
    KeepAliveTime: Date;
    OfflineTime: Date;
    RemoteInfo: string;
    DsOnline: string;
    DsStatus: string;
    DsReason: string;
    DsEncode: string;
    DsRecord: string;
    DsDeviceTime: string;
    GetDsTime: Date;
    HasAlarm: boolean;
    CreateTime: Date;
    UpTime: Date;
    NickName: string;
    SubscribeExpires: number;
  };

  /** 获取用户列表结果 */
  type TDeviceInfoList = TDeviceInfo[];
}
