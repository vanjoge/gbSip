namespace GBWeb.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        public ApiResult(int code)
        {
            this.code = code;
        }
        /// <summary>
        /// 结果代码
        /// </summary>
        public int code { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T> : ApiResult
    {
        /// <summary>
        /// 
        /// </summary>
        public ApiResult() : this(200)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        public ApiResult(int code) : base(code)
        {
        }

        /// <summary>
        /// 数据
        /// </summary>
        public T data { get; set; }


    }
}
