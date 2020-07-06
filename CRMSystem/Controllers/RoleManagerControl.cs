using System;
using System.Collections.Generic;
using System.Data;
using CRMSystem.DTOModels;
using JHC.DataManager;
using JHC.ToolKit.Base;
using JHC.ToolKit.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CRMSystem.Controllers
{
    [Route("api/role")]
    [Authorize]
    public class RoleManagerControl: Controller
    {
        private readonly ILogger<RoleManagerControl> _logger;
        private readonly DapperClient _dapperClient;
        public RoleManagerControl(ILogger<RoleManagerControl> logger, IDapperClientFactory dapperFactory)
        {
            _logger = logger;
            _dapperClient = dapperFactory.GetClient("mysql1");
        }

        /// <summary>
        /// 保存角色基本信息
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPost,Route("saverole")]
        public ServiceResult SaveRole([FromBody] RoleObjectDTO role) {
            ServiceResult sr = new ServiceResult();
            var roleid = role.roleid;
            var roleName = role.rolename;
            var sql = "";
            if (roleid == "" || roleid == null)
            {
                roleid = _dapperClient.GetSequence("user_roles").ToString().PadLeft(8, '0');
                sql = string.Format("insert into user_roles VALUES('{0}','{1}','{2}')", roleid, roleName, DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else {
                sql = string.Format("update user_roles set rolename='{0}' where roleid='{1}'", roleName, roleid);
            }
            var cnt=_dapperClient.Execute(sql,null);
            if (cnt >= 0)
            {
                sr.IsSuccess();
            }
            else {
                sr.IsFailed("保存失败");
            }
            return sr;
        }

        /// <summary>
        /// 删除权限
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPost, Route("delrole")]
        public ServiceResult DeleteRole([FromBody] RoleObjectDTO role)
        {
            ServiceResult sr = new ServiceResult();
            var roleid = role.roleid;
            var roleName = role.rolename;

            if (roleid == "" || roleid == null)
            {
                sr.IsFailed("角色不存在");
                return sr;
            }
            var sql = string.Format("delete from user_roles where roleid='{0}'", roleid);
            
            var cnt = _dapperClient.Execute(sql, null);
            if (cnt >= 0)
            {
                sr.IsSuccess();
            }
            else
            {
                sr.IsFailed("删除失败");
            }
            return sr;
        }

        /// <summary>
        /// 获取角色列表
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpGet, Route("getroles")]
        public ServiceResult<List<RoleObjectDTO>> GetAllRole()
        {
            ServiceResult<List<RoleObjectDTO>> sr = new ServiceResult<List<RoleObjectDTO>>();

            var sql = string.Format("select roleid,rolename from user_roles");
            try {
                var list = _dapperClient.Query<RoleObjectDTO>(sql);
                sr.IsSuccess(list);
            }
            catch(Exception e) {
                sr.IsFailed(e.Message);
            }
            return sr;
        }


        /// <summary>
        /// 根据角色编号获取角色详情
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("getauthbyid")]
        public ServiceResult<string> GetAllRole([FromBody] RoleObjectDTO role)
        {
            ServiceResult<string> sr = new ServiceResult<string>();
            var roleid = role.roleid;

            var sql = string.Format("select roleid,funcid,ftype from user_rfunc where roleid='{0}'", roleid);
            try
            {
                JObject rjo = new JObject();
                JArray menus = new JArray();
                JArray auths = new JArray();
                var list = _dapperClient.Query<dynamic>(sql);
                var rjs = JsonConvert.SerializeObject(list);
                JArray rjArray = (JArray)JsonConvert.DeserializeObject(rjs);
                foreach (JObject jo in rjArray) {
                    var ftype = jo.GetValue("ftype").ToString();
                    if ( ftype== "0")
                    {
                        menus.Add(jo.GetValue("funcid").ToString());
                    }
                    else
                    {
                        auths.Add(jo.GetValue("funcid").ToString());
                    }
                }
                rjo["menus"] = menus;
                rjo["auths"] = auths;
                sr.IsSuccess(rjo.ToString());
            }
            catch (Exception e)
            {
                sr.IsFailed(e.Message);
            }
            return sr;
        }

        /// <summary>
        /// 保存角色详情
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPost, Route("saveauth")]
        public ServiceResult saveAuth([FromBody] RoleObjectDTO role)
        {
            ServiceResult sr = new ServiceResult();
            var roleid = role.roleid;
            var conn = _dapperClient.Connection;
            conn.Open();
            using (var s = conn.BeginTransaction())
            {
                try
                {
                    using (var cmd = new MySqlCommand()) {
                        var sql = string.Format("delete from user_rfunc where roleid='{0}'", roleid);
                        cmd.CommandText = sql;
                        cmd.Connection = (MySqlConnection)conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                        
                    var menus = role.menus;
                    var auths = role.auths;
                    foreach (var m in menus)
                    {
                        using (var cmd = new MySqlCommand())
                        {
                            var sql = string.Format("insert into user_rfunc values('{0}','{1}','{2}')", roleid, m, "0");
                            cmd.CommandText = sql;
                            cmd.Connection = (MySqlConnection)conn;
                            cmd.CommandType = CommandType.Text;
                            cmd.ExecuteNonQuery();
                        }


                            
                    }

                    foreach (var a in auths)
                    {
                        using (var cmd = new MySqlCommand())
                        {
                            var sql = string.Format("insert into user_rfunc values('{0}','{1}','{2}')", roleid, a, "1");
                            cmd.CommandText = sql;
                            cmd.Connection = (MySqlConnection)conn;
                            cmd.CommandType = CommandType.Text;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    s.Commit();
                }
                catch (Exception e)
                {
                    s.Rollback();
                    sr.IsFailed(e.Message);
                    return sr;
                }
                conn.Close();
            }
            sr.IsSuccess("");
            return sr;
        }
    }

}
