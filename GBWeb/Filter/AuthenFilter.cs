using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using GBWeb.Models;
using System.Threading.Tasks;
using GBWeb.Attribute;
using System.Linq;
using SQ.Base;
using Microsoft.Extensions.Primitives;

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
            if (HasAllowAnonymous(context)) return;

            if (context.Filters.OfType<AuthApiAttribute>().Any() || context.HttpContext.GetEndpoint()?.Metadata?.GetMetadata<AuthApiAttribute>() != null)
            {
                if (CheckAPIAuthorization(context)) return;
                context.Result = new ObjectResult(-2);
            }
            else
            {
                if (await Check(context)) return;
                context.Result = new JsonResult(new ApiResult(11001));
            }
        }
        private async Task<bool> Check(AuthorizationFilterContext context)
        {
            if (GetHeadAuthorization(context, out var auth) && await Program.sipServer.DB.CheckToken(auth))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckAPIAuthorization(AuthorizationFilterContext context)
        {
            if (string.IsNullOrWhiteSpace(Program.sipServer.Settings.APIAuthorization))
            {
                return true;
            }

            return GetHeadAuthorization(context, out var auth)
                && auth == Program.sipServer.Settings.APIAuthorization;

        }
        private bool GetHeadAuthorization(AuthorizationFilterContext context, out StringValues auth)
        {
            foreach (var p in context.HttpContext.Request.Headers)
            {
                if ("authorization".IgnoreEquals(p.Key))
                {
                    auth = p.Value;

                    return true;
                }
            }
            return false;
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
