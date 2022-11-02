declare namespace API {
  type TSuperior = {
    // 启用
    Enable: boolean;
    // 唯一ID
    Id: string;
    // 名称
    Name: string;
    // 上级国标编码
    ServerId: string;
    // 服务域
    ServerRealm: string;
    // 上级IP/域名
    Server: string;
    // 上级端口
    ServerPort: number;
    // 本地SIP国标编码
    ClientId: string;
    // 本地SIP名称
    ClientName: string;
    // SIP认证用户名
    Sipusername: string;
    // SIP认证密码
    Sippassword: string;
    // 注册有效期
    Expiry: number;
    // 注册间隔
    RegSec: number;
    // 心跳周期
    HeartSec: number;
    // 最大心跳超时次数
    HeartTimeoutTimes: number;
    // TCP/UDP
    UseTcp: boolean;
  };
  /** 获取用户列表结果 */
  type TSuperiorList = TSuperior[];
}
