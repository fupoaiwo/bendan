using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;
using BenDan.InitialConfiguration;
using Infrastructure;
using Infrastructure.Repositorys;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using static BenDan.InitialConfiguration.SwaggerHelper.CustomApiVersion;

namespace BenDan
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

            services.AddDbContext<DataContext>(options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            #region Authorization
            //添加策略鉴权模式
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Permission", policy => policy.Requirements.Add(new PolicyRequirement()));
            })
            .AddAuthentication(s =>
            {
                //添加JWT Scheme
                s.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                s.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                s.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            #endregion

            #region JWT
             .AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateLifetime = true,//是否验证失效时间
                     ClockSkew = TimeSpan.FromSeconds(30),

                     ValidateAudience = true,//是否验证Audience
                                             //ValidAudience = Const.GetValidudience(),//Audience
                                             //这里采用动态验证的方式，在重新登陆时，刷新token，旧token就强制失效了
                     AudienceValidator = (m, n, z) =>
                     {
                         return m != null && m.FirstOrDefault().Equals(Const.ValidAudience);
                     },
                     ValidateIssuer = true,//是否验证Issuer
                     ValidIssuer = Const.Domain,//Issuer，这两项和前面签发jwt的设置一致

                     ValidateIssuerSigningKey = true,//是否验证SecurityKey
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Const.SecurityKey))//拿到SecurityKey
                 };
                 options.Events = new JwtBearerEvents
                 {
                     OnAuthenticationFailed = context =>
                     {
                         //Token expired
                         if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                         {
                             context.Response.Headers.Add("Token-Expired", "true");
                         }
                         return Task.CompletedTask;
                     }
                 };
             });
            #endregion

            #region swagger
            services.AddSwaggerGen(c =>
            {

                #region old
                //c.SwaggerDoc("v1", new OpenApiInfo
                //{
                //    Version = "v1",
                //    Title = "BenDan API",
                //    Description = "基于.NET Core 3.1 + Vue2 的开源博客",
                //    Contact = new OpenApiContact
                //    {
                //        Name = "luren",
                //        Email = "lurenio@qq.com",
                //        Url = new Uri("http://bendan.org"),
                //    },
                //});
                #endregion

                //遍历出全部的版本，做文档信息展示
                typeof(ApiVersions).GetEnumNames().ToList().ForEach(version =>
                {
                    c.SwaggerDoc(version, new OpenApiInfo
                    {
                        // {ApiName} 定义成全局变量，方便修改
                        Version = version,
                        Title = "BenDan API",
                        Description = "基于.NET Core 3.1 + Vue2 的开源博客。当前版本：" + version,
                        Contact = new OpenApiContact { Name = "luren", Email = "lurenio@qq.com", Url = new Uri("http://bendan.org")}
                    });
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);


                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });
            //认证服务
            services.AddSingleton<IAuthorizationHandler, PolicyHandler>();

            #endregion

            services.AddScoped<IUsersRepository, UsersRepository>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            //启用静态文件中间件
            app.UseStaticFiles();

            //初始化数据
            InitializeDatabase.Initialize(app.ApplicationServices);

            //添加jwt验证
            app.UseAuthentication();

            #region swagger 

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api-docs/{documentName}/bendan.json";
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = "BenDan API";
                //c.SwaggerEndpoint("/api-docs/v1/bendan.json", "BenDan API V1"); //old
                //根据版本名称倒序 遍历展示
                typeof(ApiVersions).GetEnumNames().OrderByDescending(e => e).ToList().ForEach(version =>
                {
                    c.SwaggerEndpoint($"/api-docs/{version}/bendan.json", $"BenDan {version}");
                });
                c.RoutePrefix = string.Empty;
                c.IndexStream = () => GetType().Assembly.GetManifestResourceStream("BenDan.wwwroot.swagger.ui.index.html");
            });

         

            #endregion

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

          
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
