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
            //��Ӳ��Լ�Ȩģʽ
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Permission", policy => policy.Requirements.Add(new PolicyRequirement()));
            })
            .AddAuthentication(s =>
            {
                //���JWT Scheme
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
                     ValidateLifetime = true,//�Ƿ���֤ʧЧʱ��
                     ClockSkew = TimeSpan.FromSeconds(30),

                     ValidateAudience = true,//�Ƿ���֤Audience
                                             //ValidAudience = Const.GetValidudience(),//Audience
                                             //������ö�̬��֤�ķ�ʽ�������µ�½ʱ��ˢ��token����token��ǿ��ʧЧ��
                     AudienceValidator = (m, n, z) =>
                     {
                         return m != null && m.FirstOrDefault().Equals(Const.ValidAudience);
                     },
                     ValidateIssuer = true,//�Ƿ���֤Issuer
                     ValidIssuer = Const.Domain,//Issuer���������ǰ��ǩ��jwt������һ��

                     ValidateIssuerSigningKey = true,//�Ƿ���֤SecurityKey
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Const.SecurityKey))//�õ�SecurityKey
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
                //    Description = "����.NET Core 3.1 + Vue2 �Ŀ�Դ����",
                //    Contact = new OpenApiContact
                //    {
                //        Name = "luren",
                //        Email = "lurenio@qq.com",
                //        Url = new Uri("http://bendan.org"),
                //    },
                //});
                #endregion

                //������ȫ���İ汾�����ĵ���Ϣչʾ
                typeof(ApiVersions).GetEnumNames().ToList().ForEach(version =>
                {
                    c.SwaggerDoc(version, new OpenApiInfo
                    {
                        // {ApiName} �����ȫ�ֱ����������޸�
                        Version = version,
                        Title = "BenDan API",
                        Description = "����.NET Core 3.1 + Vue2 �Ŀ�Դ���͡���ǰ�汾��" + version,
                        Contact = new OpenApiContact { Name = "luren", Email = "lurenio@qq.com", Url = new Uri("http://bendan.org")}
                    });
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);


                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "���¿�����������ͷ����Ҫ���Jwt��ȨToken��Bearer Token",
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
            //��֤����
            services.AddSingleton<IAuthorizationHandler, PolicyHandler>();

            #endregion

            services.AddScoped<IUsersRepository, UsersRepository>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            //���þ�̬�ļ��м��
            app.UseStaticFiles();

            //��ʼ������
            InitializeDatabase.Initialize(app.ApplicationServices);

            //���jwt��֤
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
                //���ݰ汾���Ƶ��� ����չʾ
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
