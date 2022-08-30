using GBWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="run"></param>
        /// <returns></returns>
        protected async Task<ApiResult<T>> RetApiResult<T>(Task<T> run)
        {
            try
            {
                var data = await run;
                return RetApiResult<T>(data);
            }
            catch (System.Exception ex)
            {
                return new ApiResult<T> { code = 500, message = ex.Message.ToString() };
            }
        }
    }
}
