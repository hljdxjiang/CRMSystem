using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JHC.ToolKit.Config;
using JHC.ToolKit.Extensions;
using JHC.ToolKit.Helper;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CRMSystem.Helper
{
    public class TokenHelper
    {
        private static readonly Microsoft.Extensions.Logging.ILogger _logger = new LoggerFactory().AddLog4Net().CreateLogger<TokenHelper>();
        private static TokenHelper _inst;
        public  static TokenHelper Instance {
            get {
                if (_inst == null) {
                    lock (typeof(TokenHelper)) {
                        if (_inst == null) {
                            _inst = new TokenHelper();
                        }
                    }
                }
                    return _inst;
            }
        }


        /// <summary>
        /// 生成token
        /// </summary>
        /// <param name="uname"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public string buildToken(string uname, string roles,string refresh_token)
        {
            string ret = "";
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSercretConfig.Audience_Secret_String));
            var creds = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
            string iss = Appsettings.Instance.app(new string[] { "JwtSettings", "Issuer" });
            string aud = Appsettings.Instance.app(new string[] { "JwtSettings", "Audience" });
            var exp = Convert.ToDouble(Appsettings.Instance.app(new string[] { "JwtSettings", "Expires" }));
            DateTime expiresAt = DateTime.Now.ToCstTime().AddMinutes(exp);

            var reft = Convert.ToDouble(Appsettings.Instance.app(new string[] { "JwtSettings", "RefExpires" }));
            DateTime refExpires = DateTime.Now.ToCstTime().AddMinutes(reft);


            IEnumerable<Claim> claims = new Claim[] {
                new Claim(ClaimTypes.Name,uname),
                new Claim("RefToken",refresh_token),
                new Claim("RefExpires",refExpires.ToString()),
                new Claim(ClaimTypes.Role,roles),
                new Claim(ClaimTypes.Expiration,expiresAt.ToString())
            };

            var jwt = new JwtSecurityToken(
                issuer: iss,
                audience:aud,
                expires: expiresAt,
                claims: claims,
                signingCredentials: creds);

            var jwtHandler = new JwtSecurityTokenHandler();
            ret = jwtHandler.WriteToken(jwt);

            return ret;
        }

        /// <summary>
        /// 解析token与reftoken
        /// 刷新token时判断token是否被篡改或者过期
        /// </summary>
        /// <param name="token"></param>
        /// <param name="reftoken"></param>
        /// <returns></returns>
        public bool SerializeJwt(string token,string reftoken)
        {
            var b = false;

            var jwtHandler = new JwtSecurityTokenHandler();
            if (String.IsNullOrEmpty(token) || String.IsNullOrEmpty(reftoken))
            {
                return false;
            }
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(token);
            object tref, trefexp;
            try
            {
                jwtToken.Payload.TryGetValue("RefToken", out tref);
                jwtToken.Payload.TryGetValue("RefExpires", out trefexp);
                if (tref.ToString() != reftoken)
                {
                    return false;   
                }
                var reft = Convert.ToDateTime(trefexp);
                var exp = Convert.ToDouble(Appsettings.Instance.app(new string[] { "JwtSettings", "RefExpires" }));
                DateTime ndt = DateTime.Now.ToCstTime().AddMinutes(exp);
                if (DateTime.Compare(reft,ndt)<=0) {
                    b = true;
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                b = false;
            }


            return b;
        }

    }
}
