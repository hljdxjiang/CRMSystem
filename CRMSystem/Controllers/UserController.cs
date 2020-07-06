using System;
using System.Collections.Generic;
using System.Data;
using CRMSystem.DTOModels;
using CRMSystem.Helper;
using JHC.DataManager;
using JHC.ToolKit.Base;
using JHC.ToolKitExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CRMSystem.Controllers
{
    [Route("api/user")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly DapperClient _dapperClient;
        public UserController(ILogger<UserController> logger, IDapperClientFactory dapperFactory)
        {
            _logger = logger;
            _dapperClient = dapperFactory.GetClient("mysql1");
        }

        /// <summary>
        /// 根据groupid获取用户列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("getuserinfo")]
        public ServiceResult<UserInfoDTO> GetUserByID()
        {
            ServiceResult<UserInfoDTO> sr = new ServiceResult<UserInfoDTO>();
            LoggerTimes lt = new LoggerTimes();
            var id = HttpContext.Request.Query["userid"];
            _logger.LogInformation("GetUserByID begin Groupid is<{0}>", id);
            var b = StringExtensions.SqlValidate(id);
            if (!b)
            {
                sr.IsFailed("获取用户失败，请检查传入参数");
            }
            else
            {
                string sql = string.Format("select addr,DATE_FORMAT(birthday,'%Y-%m-%d') birthday,email,emecontact,emephone,gender,DATE_FORMAT(hiredate,'%Y-%m-%d') hiredate,idno,idtype" +
                    ",mobile,submobile,uname,userid,groupid,status,roleid FROM user_users where userid='{0}'",
                    id);
                try
                {
                    var list = _dapperClient.QueryFirst<UserInfoDTO>(sql);
                    sr.IsSuccess(list);
                }
                catch (Exception e)
                {
                    _logger.LogError("GetUserByID end with Exception Groupid is<{0}> Exception is <{1}> Cost <{2}>", id, e.Message, lt.ElapsedMilliseconds());
                    sr.IsFailed(e.Message);
                }
            }
            _logger.LogInformation("GetUserByID End Groupid is<{0}> cost<{1}>", id, lt.ElapsedMilliseconds());
            return sr;
        }


        /// <summary>
        /// 根据groupid获取用户列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("getuserbyid")]
        public ServiceResult<List<UserInfoDTO>> GetUserByGroupID() {
            ServiceResult<List<UserInfoDTO>> sr = new ServiceResult<List<UserInfoDTO>>();
            LoggerTimes lt = new LoggerTimes();
            var id = HttpContext.Request.Query["groupid"];
            _logger.LogInformation("GetUserByID begin Groupid is<{0}>", id);
            var b=StringExtensions.SqlValidate(id);
            if (!b)
            {
                sr.IsFailed("获取用户失败，请检查传入参数");
            }
            else {
                var list = getuser(id);
                if (list == null) {
                    sr.IsFailed("获取用户信息失败");
                }
                sr.IsSuccess(list);
            }
            _logger.LogInformation("GetUserByID End Groupid is<{0}> cost<{1}>", id,lt.ElapsedMilliseconds());
            return sr;
        }

        private List<UserInfoDTO> getuser(string userid) {
            LoggerTimes lt = new LoggerTimes();
            string sql = string.Format("select addr,DATE_FORMAT(birthday,'%Y-%m-%d') birthday,email,emecontact,emephone,gender," +
                   "DATE_FORMAT(hiredate,'%Y-%m-%d') hiredate,idno,idtype,mobile,submobile,uname,userid,groupid,status,roleid FROM user_users where groupid='{0}'",
                   userid);
            try
            {
                var list = _dapperClient.Query<UserInfoDTO>(sql);
                return list;
                
            }
            catch (Exception e)
            {
                _logger.LogError("GetUserByID end with Exception Groupid is<{0}> Exception is <{1}> Cost <{2}>", userid, e.Message, lt.ElapsedMilliseconds());
                return null;
            }
        }


        /// <summary>
        /// 删除用户
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("getuserbyvalues")]
        public ServiceResult<UserInfoDTO> GetUserByNameOrID()
        {
            ServiceResult<UserInfoDTO> sr = new ServiceResult<UserInfoDTO>();
            string id = HttpContext.Request.Query["val"];
            id = id.Split("-")[0];  
            _logger.LogInformation("Get User begin Values is<{0}>", id);
            LoggerTimes lt = new LoggerTimes();
            var b = StringExtensions.SqlValidate(id);
            if (!b)
            {
                sr.IsFailed("获取用户失败，请检查传入参数");
                return sr;
            }
            string sql = string.Format("select * FROM user_users where userid='{0}' or uname='{0}'",
                id);
            try
            {
                var user = _dapperClient.QueryFirst<UserInfoDTO>(sql);
                sr.IsSuccess(user);
                
            }
            catch (Exception e)
            {
                _logger.LogError("Delete User  with Exception Userid is<{0}> Exception is <{1}> Cost <{2}>", id, e.Message, lt.ElapsedMilliseconds());
                sr.IsFailed(e.Message);
            }
            _logger.LogInformation("Delete User  End Userid is<{0}> cost<{1}>", id, lt.ElapsedMilliseconds());
            return sr;
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("deluserbyid")]
        public ServiceResult DelUserByID()
        {
            ServiceResult sr = new ServiceResult ();
            var id = HttpContext.Request.Query["userid"];
            _logger.LogInformation("Delete User begin Userid is<{0}>", id);
            LoggerTimes lt = new LoggerTimes();
            var b = StringExtensions.SqlValidate(id);
            if (!b)
            {
                sr.IsFailed("获取用户失败，请检查传入参数");
                return sr;
            }
            string sql = string.Format("delete FROM user_users where userid='{0}'",
                id);
            try
            {
                var cnt = _dapperClient.Execute(sql,null);
                if (cnt > 0)
                {
                    sr.IsSuccess();
                }
                else {
                    sr.IsFailed(string.Format("删除失败，删除数据条数{0}条", cnt));
                }
                
            }
            catch (Exception e)
            {
                _logger.LogError("Delete User  with Exception Userid is<{0}> Exception is <{1}> Cost <{2}>", id, e.Message, lt.ElapsedMilliseconds());
                sr.IsFailed(e.Message);
            }
            _logger.LogInformation("Delete User  End Userid is<{0}> cost<{1}>", id, lt.ElapsedMilliseconds());
            return sr;
        }

        /// <summary>
        /// 禁用启用用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost, Route("disableuserbyid")]
        public ServiceResult DisableUserByID([FromBody] UserInfoDTO user)
        {
            ServiceResult sr = new ServiceResult();
            var id = user.Userid;
            var status = user.Status;
            var b = StringExtensions.SqlValidate(id);
            if (!b)
            {
                sr.IsFailed("修改用户失败，请检查传入参数");
                return sr;
            }
            string sql = string.Format("update  user_users set status={1} where userid='{0}'",
                id,status);
            try
            {
                var cnt = _dapperClient.Execute(sql,null);
            }
            catch (Exception e)
            {
                sr.IsFailed(e.Message);
            }
            return sr;
        }

        /// <summary>
        /// 保存、新建用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost, Route("save")]
        public ServiceResult SaveUser([FromBody] UserInfoDTO user)
        {
            var id = user.Userid;
            LoggerTimes lt = new LoggerTimes();
            ServiceResult sr = new ServiceResult();

            _logger.LogInformation("Save User begin Userid is<{0}>", id);


            bool isfirst = true;
            var upstr = "";var key = "";var  values="";
            foreach (System.Reflection.PropertyInfo info in user.GetType().GetProperties())
            {
                var val = info.GetValue(user);
                var name = info.Name;
                if (string.IsNullOrWhiteSpace((string)val) || val == null)
                {
                    continue;
                }
                if (isfirst)
                {
                    key += name;
                    values += "'" + val + "'";
                    upstr += name + "='" + val + "'";
                }
                else
                {
                    key += "," + name;
                    values += ",'" + val + "'";
                    upstr += ",'" + name + "='" + val + "'";
                }
                isfirst = false;
            }
            string sql = string.Format("insert into user_users({0})values({1})", key, values);
            var list = getuser(id);
            if (list.Count > 0) {
                sql = string.Format("update user_users set {0} where userid='{1}'", upstr, id);
            }
            try
            {
                var cnt = _dapperClient.Execute(sql, null);
                if (cnt <= 0)
                {
                    sr.IsFailed("保存用户信息失败");
                }
                else {
                    sr.IsSuccess();
                }
            }
            catch (Exception e) {
                sr.IsFailed(e.Message);
            }
            return sr;
        }

        /// <summary>
        /// 保存、新建用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost, Route("changepwd")]
        public ServiceResult ChangePasswd([FromBody] PasswdRequestDTO user)
        {
            ServiceResult sr = new ServiceResult();
            string id = user.Userid;
            if (id == null || id == "") {
                HttpContext.Request.Cookies.TryGetValue("_userid", out id);
            }

            _logger.LogInformation("ChangePasswd begin Userid is<{0}>", id);

            string sql = string.Format("select userid,lastpasswd,sendpasswd,passwd from user_users where userid='{0}'", id);
            var list = _dapperClient.QueryFirst<dynamic>(sql);

            var rjs = JsonConvert.SerializeObject(list);
            JObject jo = (JObject)JsonConvert.DeserializeObject(rjs);
            string newpasswd = EncryptExtensions.EncodeMd5String(user.NewPasswd);
            string orgpasswd= EncryptExtensions.EncodeMd5String(user.OrgPasswd);
            if (orgpasswd != jo.GetValue("passwd").ToString()) {
                sr.IsFailed("原密码输入有误");
                return sr;
            }
            if (newpasswd == jo.GetValue("lastpasswd").ToString() || newpasswd == jo.GetValue("sendpasswd").ToString()) {
                sr.IsFailed("请使用与最近两次密码不同的密码");
                return sr;
            }
            sql = string.Format("update user_users set passwd='{0}',lastpasswd=passwd,sendpasswd=lastpasswd where userid='{1}'", newpasswd, id);
            try
            {
                var cnt = _dapperClient.Execute(sql, null);
                if (cnt <= 0)
                {
                    sr.IsFailed("密码修改失败");
                }
                else
                {
                    sr.IsSuccess();
                }
            }
            catch (Exception e)
            {
                sr.IsFailed(e.Message);
            }
            return sr;
        }
    }
}
