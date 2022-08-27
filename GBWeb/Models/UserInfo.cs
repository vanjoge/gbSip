using System.Collections.Generic;
using System;

namespace GBWeb.Models
{

    /// <summary>
    /// 用户信息
    /// </summary>
    public class UserInfo
    {
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Psalt { get; set; }
        public string NickName { get; set; }
        public string HeadImg { get; set; }
        public string LoginIp { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public List<int> Roles { get; set; }
        public string DepartmentName { get; set; }
    }
}
