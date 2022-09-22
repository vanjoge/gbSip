declare namespace API {
  type Menu = {
    CreateTime: Date;
    UpdateTime: Date;
    Id: number;
    ParentId: number;
    Name: string;
    Router: string;
    Perms: string;
    /** 当前菜单类型 0: 目录 | 1: 菜单 | 2: 权限 */
    Type: 0 | 1 | 2;
    Icon: string;
    OrderNum: number;
    ViewPath: string;
    Keepalive: boolean;
    IsShow: boolean;
  };

  type PermMenu = {
    Menus: Menu[];
    Perms: string[];
  };

  type AdminUserInfo = {
    CreateTime: Date;
    UpdateTime: Date;
    Id: number;
    DepartmentId: number;
    Name: string;
    Username: string;
    Password: string;
    Psalt: string;
    NickName: string;
    HeadImg: string;
    LoginIp: string;
    Email: string;
    Phone: string;
    Remark: string;
    Status: number;
    Roles: number[];
    DepartmentName: string;
  };
}
