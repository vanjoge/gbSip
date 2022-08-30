using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SipServer.Models
{
    /// <summary>
    /// List数据分页对象
    /// </summary>
    public class DPager<T>
    {
        public DPager(List<T> list, int page, int size, int total)
        {
            this.list = list;
            this.pagination = new Pagination
            {
                page = page,
                total = total,
                size = size,
            };
        }

        /// <summary>
        /// 数据
        /// </summary>
        public List<T> list { get; set; }
        /// <summary>
        /// 分页信息
        /// </summary>
        public Pagination pagination { get; set; }
    }
    public class Pagination
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int page { set; get; }
        /// <summary>
        /// 
        /// </summary>

        public int size { set; get; }
        /// <summary>
        /// 总数
        /// </summary>

        public int total { set; get; }
    }
}
