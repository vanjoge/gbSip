declare namespace API {
  // type T_Catalog = {
  //   ChannelId: string;
  //   DeviceId: string;
  //   ParentId: string;
  //   Name: string;
  //   Manufacturer: string;
  //   Model: string;
  //   Owner: string;
  //   CivilCode: string;
  //   Block: string;
  //   Address: string;
  //   Parental: boolean;
  //   BusinessGroupId: string;
  //   SafetyWay: number;
  //   RegisterWay: number;
  //   CertNum: string;
  //   Certifiable: boolean;
  //   ErrCode: number;
  //   EndTime: Date;
  //   Secrecy: boolean;
  //   Ipaddress: string;
  //   Port: number;
  //   Password: string;
  //   Status: string;
  //   Longitude: number;
  //   Latitude: number;
  //   RemoteEp: string;
  //   NickName: string;
  //   NetType: number;
  //   TalkType: number;
  //   Online: boolean;
  //   OnlineTime: Date;
  //   OfflineTime: Date;
  // };
  // type ChannelConf = {
  //   NickName: string;
  //   NetType: number;
  //   TalkType: number;
  // };
  // type TChannelResult = {
  //   Catalog: T_Catalog;
  //   Conf: ChannelConf;
  // };

  type TChannel = {
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
    NickName: string;
    NetType: number;
    TalkType: number;
    RTVSVideoServer: string;
    RTVSVideoPort: number;
    PlayerMode: number;
    DType: number;
  };

  /** 获取用户列表结果 */
  type TChannelList = TChannel[];
}
