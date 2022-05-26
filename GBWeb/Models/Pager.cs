using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GBWeb.Models
{
    /// <summary>
    /// List数据分页对象
    /// </summary>
    public class Pager<T>
    {
        public int currentPage { set; get; }

        public int pageSize { set; get; }

        public int totalPage { set; get; }

        public int totalData { set; get; }

        private List<T> Results = new List<T>();

        public List<T> data
        {
            set { value = Results; }
            get { return Results; }
        }
        /// <summary>
        /// 分页原始数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="deviceInfos"></param>
        public void Paging(int page, int pageSize, List<T> data)
        {
            this.pageSize = pageSize;
            this.currentPage = page;
            this.totalData = data.Count;
            if (data.Count % this.pageSize == 0)
            {
                totalPage = data.Count / this.pageSize;
            }
            else
            {
                totalPage = data.Count / this.pageSize + 1;
            }

            if (currentPage > totalPage)
            {
                currentPage = totalPage;
            }

            if (data.Count > 0)
            {
                int left = data.Count - (currentPage - 1) * this.pageSize;
                Results = data.GetRange((currentPage - 1) * this.pageSize, left > this.pageSize ? this.pageSize : left);
            }

        }

        /// <summary>
        /// 分页原始数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="total"></param>
        /// <param name="data"></param>
        public void Paging(int page, int pageSize, int total, List<T> data)
        {
            this.pageSize = pageSize;
            this.currentPage = page;
            this.totalData = total;
            if (this.pageSize == 0)
            {
                totalPage = 1;
            }
            else if (total % this.pageSize == 0)
            {
                totalPage = total / this.pageSize;
            }
            else
            {
                totalPage = total / this.pageSize + 1;
            }

            if (currentPage > totalPage)
            {
                currentPage = totalPage;
            }

            if (total > 0)
            {
                int left = total - (currentPage - 1) * this.pageSize;
                Results = data;
            }

        }

    }
}
