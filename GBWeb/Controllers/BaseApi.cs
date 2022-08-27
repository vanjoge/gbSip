using GBWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace GBWeb.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseApi : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        protected ApiResult<T> RetApiResult<T>(T data)
        {
            return new ApiResult<T> { data = data };
        }
    }
}
