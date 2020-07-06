using System;
using System.Text;
using System.Threading.Tasks;
using JHC.ToolKit.Config;
using JHC.ToolKit.Helper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CRMSystem.Helper
{
    public static class JwtConfiguration
    {
        public static void AddJwtConfiguration(this IServiceCollection services)
        {
            var key= AppSercretConfig.Audience_Secret_String;
            var auth = Appsettings.Instance.app(new string[] { "JwtSettings", "Issuer" });
            if (true)
            {
                services.AddAuthentication(options => {
                    options.DefaultAuthenticateScheme = "JwtBearer";
                    options.DefaultChallengeScheme = "JwtBearer";
                }).AddJwtBearer("JwtBearer", options =>
                {
                    //options.Audience = Appsettings.Instance.app(new string[] { "JwtSettings", "Audience" });
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Add("act", "expired");
                            }
                            return Task.CompletedTask;
                        }
                    };
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // The signing key must match!
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes(AppSercretConfig.Audience_Secret_String)),

                        // Validate the JWT Issuer (iss) claim
                        ValidateIssuer = true,
                        ValidIssuer = Appsettings.Instance.app(new string[] { "JwtSettings", "Issuer" }),

                        // Validate the JWT Audience (aud) claim
                        ValidateAudience = true,
                        ValidAudience = Appsettings.Instance.app(new string[] { "JwtSettings", "Audience" }),

                        // Validate the token expiry 
                        // ValidateLifetime = true,

                        // If you want to allow a certain amount of clock drift, set that here
                        //ClockSkew = TimeSpan.Zero,
                        ClockSkew = TimeSpan.FromMinutes(5),
                        RequireExpirationTime =true,
                    };
                });
            }
        }
    }
}
