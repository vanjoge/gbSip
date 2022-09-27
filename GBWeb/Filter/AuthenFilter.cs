using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using GBWeb.Models;

namespace GBWeb.Filter
{
    public class AuthenFilter : IAuthorizationFilter
    {
        /// <summary>
        /// 每个action执行之前都会进入这个方法
        /// </summary>
        /// <param name="context"></param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //如果不通过认证 重定向到/Login/User页
            if (Check(context) || HasAllowAnonymous(context)) return;
            context.Result = new JsonResult(new ApiResult(11001));
        }
        private bool Check(AuthorizationFilterContext context)
        {
            if (context.HttpContext.Request.Headers.TryGetValue("authorization", out var auth))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 用于判断Action有没有AllowAnonymous标签
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool HasAllowAnonymous(FilterContext context)
        {
            var filters = context.Filters;
            for (var i = 0; i < filters.Count; i++)
            {
                if (filters[i] is IAllowAnonymousFilter)
                {
                    return true;
                }
            }

            var endpoint = context.HttpContext.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return true;
            }

            return false;
        }
    }
}
