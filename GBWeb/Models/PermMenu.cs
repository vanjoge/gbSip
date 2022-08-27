using System;
using System.Collections.Generic;

namespace GBWeb.Models
{
    public class PermMenu
    {
        public List<Menu> Menus { get; set; }
        public List<string> Perms { get; set; }
    }

    public class Menu
    {
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public string Router { get; set; }
        public string Perms { get; set; }
        /// <summary>
        /// 当前菜单类型 0: 目录 | 1: 菜单 | 2: 权限
        /// </summary>
        public int Type { get; set; }
        public string Icon { get; set; }
        public int OrderNum { get; set; }
        public string ViewPath { get; set; }
        public bool Keepalive { get; set; }
        public bool IsShow { get; set; }
    };

}
