using GBWeb.Filter;
using IGeekFan.AspNetCore.Knife4jUI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace GBWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllersWithViews(option =>
                {
                    option.Filters.Add(typeof(AuthenFilter));
                })
                .AddJsonOptions(config =>
                {
                    config.JsonSerializerOptions.PropertyNamingPolicy = null;
                    config.JsonSerializerOptions.Converters.Add(new DatetimeJsonConverter());
                    config.JsonSerializerOptions.IgnoreNullValues = true;
                    //config.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });

            if (Program.sipServer.Settings.SwaggerDoc)
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("api", new OpenApiInfo { Title = "API", Version = "v1" });
                    c.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, "GBWeb.xml"), true);
                    c.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, "GB28181.xml"), true);
                    c.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, "SipServer.xml"), true);

                    c.OperationFilter<SecurityFilter>();

                    c.AddSecurityDefinition("authorization", new OpenApiSecurityScheme
                    {
                        Description = "授权",
                        Name = "authorization",//默认的参数名称
                        In = ParameterLocation.Header,//默认存放Authorization信息的位置(请求头中)
                        Type = SecuritySchemeType.ApiKey,
                    });
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            if (Program.sipServer.Settings.SwaggerDoc)
            {
                app.UseMiddleware<SwaggerAuthMiddleware>();
                // 添加Swagger有关中间件
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/api/swagger.json", "API");
                });
                //Swagger使用自定义UI
                app.UseKnife4UI(c =>
                {
                    c.RoutePrefix = "Help";
                    c.SwaggerEndpoint($"../swagger/api/swagger.json", "API");
                });
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public class DatetimeJsonConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (DateTime.TryParse(reader.GetString(), out DateTime date))
                        return date;
                }
                return reader.GetDateTime();
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        public class SwaggerAuthMiddleware
        {
            private readonly RequestDelegate _next;

            public SwaggerAuthMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            public async Task Invoke(HttpContext context)
            {
                if (context.Request.Path.HasValue)
                {
                    var path = context.Request.Path.Value.ToLower();
                    if (
                        (context.Request.Path.StartsWithSegments("/swagger") || context.Request.Path.StartsWithSegments("/help") || path.EndsWith("/swagger-resources"))
                        && !await AuthenFilter.Check(context))
                    {
                        //context.Response.Redirect("/Login/SignIn");
                        // 如果没有登录，重定向到登录页面或返回未授权状态
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Unauthorized - Please login to access Swagger.");
                        return;
                    }
                }

                await _next(context);
            }
        }
    }
}
