using System;
using System.Collections.Generic;
using CRMSystem.Filters;
using CRMSystem.Helper;
using JHC.DataManager;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace CRMSystem
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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo", Version = "V1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}

                    }
                });

            });
            var connets = Configuration.GetSection("DbConnedtions").GetChildren();
            var dbDic = new Dictionary<string, DapperClient>();
            foreach (var item in connets)
            {
                string name = item.GetSection("name").Value;
                string type = item.GetSection("dbtype").Value;
                string constr = item.GetSection("connectstring").Value;
                ConnectionConfig conf = new ConnectionConfig();
                conf.ConnectionString = constr;
                switch (type.ToLower())
                {
                    case "mysql":
                        conf.DbType = DbStoreType.MySql;
                        break;
                    case "oracle":
                        conf.DbType = DbStoreType.Oracle;
                        break;
                    case "sqlite":
                        conf.DbType = DbStoreType.Sqlite;
                        break;
                    case "sqlserver":
                        conf.DbType = DbStoreType.SqlServer;
                        break;
                }
                DapperClient client = new DapperClient(conf);
                dbDic.Add(name, client);
            }
            services.AddClient(dbDic);

            //这段代码是加过滤器的。。好像不生效。回头再试试。
            //Action<MvcOptions> filters = new Action<MvcOptions>(r =>
            //{
            //    r.Filters.Add(typeof(MyAuthorization));
            //});
            //services.AddMvc(filters);
            services.AddJwtConfiguration();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            

            app.UseRouting();

            //app.UseIdentityServer();


            app.UseAuthentication();//认证	
            //app.UseAuthorization();//授权
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo V1");
            });
            //app.UseHttpsRedirection();

            app.UseCors(
                options => options.AllowAnyOrigin().AllowAnyMethod()
            );

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
