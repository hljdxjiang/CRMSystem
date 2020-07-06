
using Microsoft.AspNetCore.Mvc;
using CRMSystem.DTOModels;
using System;
using Microsoft.Extensions.Logging;
using JHC.DataManager;
using JHC.ToolKitExtensions;
using JHC.ToolKit.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CRMSystem.Helper;
using Microsoft.AspNetCore.Http;
using JHC.ToolKit.Extensions;
using System.Collections.Generic;

namespace CRMSystem.Controllers
{
    //[AllowAnonymous]
    [Route("api/auth")]
    public class TokenController : Controller
    {
        private readonly ILogger<TokenController> _logger;
        private readonly DapperClient _dapperClient;

        public TokenController(ILogger<TokenController> logger,IDapperClientFactory dapperFactory)
        {
            _logger = logger;
            _dapperClient = dapperFactory.GetClient("mysql1");
        }

            //传入用户名密码获取
        [HttpPost,Route("lgoin")]
        public ServiceResult<LoginUser> Login([FromBody] LoginRequestDTO user)
        {
            ServiceResult<LoginUser> rs = new ServiceResult<LoginUser>();
            if (user.userid == "" || user.password == "") {
                rs.IsFailed("用户名密码不可为空");
                return rs;
            }
            var sql = String.Format("select userid,uname,roleid,passwd from user_users where userid='{0}' and passwd='{1}'",
                user.userid, EncryptExtensions.EncodeMd5String(user.password));
            System.Console.WriteLine(sql);
            var result = _dapperClient.Query<dynamic>(sql);
            if (result!=null && result.Count <= 0) {
                rs.IsFailed("用户名密码不正确");
                return rs;
            }
            var js = JsonConvert.SerializeObject(result);
            JArray jArray = (JArray)JsonConvert.DeserializeObject(js);
            JToken jObj = (JToken)jArray[0];
            JObject jo = (JObject)jArray[0];
            Console.WriteLine(jObj);
            Console.WriteLine(jo.GetValue("roleid"));


            var roleid = jo.GetValue("roleid").ToString();

            sql = string.Format("select * from user_rfunc where roleid='{0}'", roleid);
            result = _dapperClient.Query<dynamic>(sql);

            string refreshToken = Guid.NewGuid().ToString("N");
            string token = TokenHelper.Instance.buildToken(user.userid, roleid, refreshToken);

            CookieOptions cookieOptions = new CookieOptions();
            cookieOptions.Domain = HttpContext.Request.Host.Host; //设置domain让cookie共享
            cookieOptions.Expires = DateTime.Now.ToCstTime().AddDays(1);//设置过期时间

            HttpContext.Response.Cookies.Append("_token", token, cookieOptions);
            HttpContext.Response.Cookies.Append("_userid", user.userid, cookieOptions);
            HttpContext.Response.Cookies.Append("_user_roles", roleid, cookieOptions);
            HttpContext.Response.Cookies.Append("_refresh_token", refreshToken, cookieOptions);
            var rjs= JsonConvert.SerializeObject(result);
            LoginUser lu = new LoginUser()
            {
                Userid = user.userid,
                Token = token,
                RefToken = refreshToken,
                RoleID = roleid,
                Roles = rjs,
                RefreshTime = DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss")
            };
            rs.IsSuccess(lu);
            return rs;
        }


        /// <summary>
        /// 刷新token
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("getauths")]
        public ServiceResult<List<AuthResponseDTO>> GetAllAuth()
        {
            ServiceResult<List<AuthResponseDTO>> rs = new ServiceResult<List<AuthResponseDTO>>();
            string userid;
            HttpContext.Request.Cookies.TryGetValue("_userid", out userid);
            var sql = string.Format("select * from user_rfunc where roleid =(select roleid from user_users where userid='{0}')", userid);
            try
            {
                var list = _dapperClient.Query<AuthResponseDTO>(sql, null);
                rs.IsSuccess(list);
            }
            catch (Exception e) {
                rs.IsFailed(e.Message);
            }
            return rs;


        }

    /// <summary>
    /// 刷新token
    /// </summary>
    /// <returns></returns>
    [HttpGet, Route("refreshtokeb")]
        public ServiceResult<LoginUser> Refresh() {
            ServiceResult<LoginUser> rs = new ServiceResult<LoginUser>();
            string token,reftk,userid,roles,roleid;
            HttpContext.Request.Cookies.TryGetValue("_token", out token);
            HttpContext.Request.Cookies.TryGetValue("_refresh_token", out reftk);
            HttpContext.Request.Cookies.TryGetValue("_user_roles", out roleid);



            if (TokenHelper.Instance.SerializeJwt(token, reftk))
            {
                HttpContext.Request.Cookies.TryGetValue("_userid", out userid);
                HttpContext.Request.Cookies.TryGetValue("_user_roles", out roles);
                string nreftk = Guid.NewGuid().ToString("N");
                string ntk = TokenHelper.Instance.buildToken(userid, roles, nreftk);
                LoginUser lu = new LoginUser()
                {
                    Userid = userid,
                    Token = ntk,
                    RefToken = nreftk,
                    RefreshTime = DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss")
                };
                CookieOptions cookieOptions = new CookieOptions();
                cookieOptions.Domain = HttpContext.Request.Host.Host; //设置domain让cookie共享
                cookieOptions.Expires = DateTime.Now.ToCstTime().AddDays(1);//设置过期时间

                HttpContext.Response.Cookies.Append("_token", ntk, cookieOptions);
                HttpContext.Response.Cookies.Append("_userid", userid, cookieOptions);
                HttpContext.Response.Cookies.Append("_user_roles", roleid, cookieOptions);
                HttpContext.Response.Cookies.Append("_refresh_token", nreftk, cookieOptions);
                rs.IsSuccess(lu);
            }
            else {
                //刷新token失败，清除cookie
                CookieOptions cookieOptions = new CookieOptions();
                cookieOptions.Domain = HttpContext.Request.Host.Host; //设置domain让cookie共享
                cookieOptions.Expires = DateTime.Now.ToCstTime().AddDays(-1);//设置过期时间
                HttpContext.Response.Cookies.Append("_token", "", cookieOptions);
                HttpContext.Response.Cookies.Append("_userid", "", cookieOptions);
                HttpContext.Response.Cookies.Append("_user_roles", "", cookieOptions);
                HttpContext.Response.Cookies.Append("_refresh_token", "", cookieOptions);
                rs.IsFailed("用户信息异常");
            }
            return rs;
        }
    }
}
