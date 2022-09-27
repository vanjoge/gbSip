using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GBWeb.Filter
{
    internal class SecurityFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context != null && operation != null)
            {
                var anonymous = context.MethodInfo
                        .GetCustomAttributes(true)
                        .OfType<AllowAnonymousAttribute>().Any();

                if (!anonymous)
                {
                    var scheme = new OpenApiSecurityScheme()
                    {
                        Reference = new OpenApiReference() { Type = ReferenceType.SecurityScheme, Id = "authorization" }
                    };

                    operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement {
                        {
                            scheme,
                            new string[] { }
                        }
                    }
                };
                }
            }
        }
    }
}
