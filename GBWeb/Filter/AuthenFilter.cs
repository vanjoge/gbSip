using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using GBWeb.Models;
using System.Threading.Tasks;

namespace GBWeb.Filter
{
    public class AuthenFilter : IAsyncAuthorizationFilter
    {
        /// <summary>
        /// 每个action执行之前都会进入这个方法
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (HasAllowAnonymous(context) || await Check(context)) return;
            context.Result = new JsonResult(new ApiResult(11001));
        }
        private async Task<bool> Check(AuthorizationFilterContext context)
        {
            if (context.HttpContext.Request.Headers.TryGetValue("authorization", out var auth) && await Program.sipServer.DB.CheckToken(auth))
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
        private bool HasAllowAnonymous(FilterContext context)
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
