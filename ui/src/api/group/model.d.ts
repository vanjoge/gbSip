declare namespace API {
  type TGroup = {
    // 分组ID
    GroupId: string;
    // 上级ID
    ParentId: string;
    // 分组名称
    Name: string;
    // 查询路径 /分割
    Path: string;
  };
  /** 获取分组列表结果 */
  type TGroupList = TGroup[];

  type TGroupChannel = {
    ChannelId: string;
    DeviceId: string;
    ParentId: string;
    Name: string;
    Manufacturer: string;
    Model: string;
    Owner: string;
    CivilCode: string;
    Block: string;
    Address: string;
    Parental: boolean;
    BusinessGroupId: string;
    SafetyWay: number;
    RegisterWay: number;
    CertNum: string;
    Certifiable: boolean;
    ErrCode: number;
    EndTime: Date;
    Secrecy: boolean;
    Ipaddress: string;
    Port: number;
    Password: string;
    Status: string;
    Longitude: number;
    Latitude: number;
    RemoteEp: string;
    NickName: string;
    NetType: number;
    TalkType: number;
    Online: boolean;
    OnlineTime: Date;
    OfflineTime: Date;
    CustomChannelId: string;
    GroupId: string;
    SKey: string;
    OnlyBind: boolean;
  };
  type TGroupChannelList = TGroupChannel[];

  type TBindSuperior = {
    // 上级ID
    SuperiorId: string;
    // 分组ID
    GroupId: string;
    // 分组名称
    Name: string;
    // 包含全部下级
    HasChild: boolean;
    // 共享/取消
    Cancel: boolean;
  };
}
