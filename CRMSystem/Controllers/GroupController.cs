using System;
using System.Collections.Generic;
using CRMSystem.DTOModels;
using JHC.DataManager;
using JHC.ToolKit.Base;
using JHC.ToolKit.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace CRMSystem.Controllers
{
    [Route("api/group")]
    [Authorize]
    public class GroupController:Controller
    {
        private readonly ILogger<GroupController> _logger;
        private readonly DapperClient _dapperClient;

        public GroupController(ILogger<GroupController> logger, IDapperClientFactory dapperFactory)
        {
            _logger = logger;
            _dapperClient = dapperFactory.GetClient("mysql1");
        }

        [HttpGet, Route("getgroups")]
        public ServiceResult<List<RoleRequestDTO>> GetAllRole()
        {
            ServiceResult<List<RoleRequestDTO>> sr = new ServiceResult<List<RoleRequestDTO>>();

            JArray rja = new JArray();

            var sql = string.Format("select groupname as title,groupid as 'key',grouppid,managerid from user_groups");
            try
            {
                var list = _dapperClient.Query<RoleRequestDTO>(sql);
                sr.IsSuccess(list);
            }
            catch (Exception e)
            {
                sr.IsFailed(e.Message);
            }
            return sr;
        }

        /// <summary>
        /// 保存分组
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPost, Route("savegroup")]
        public ServiceResult SaveRole([FromBody] RoleRequestDTO role)
        {
            ServiceResult sr = new ServiceResult();

            var groupid = role.Key;
            var groupname = role.Title;
            var grouppid = role.Grouppid;
            var sql = "";
            if (groupid == "" || groupid == null) {
                groupid=_dapperClient.GetSequence("user_groups").ToString().PadLeft(8, '0');
                sql = string.Format("insert into user_groups values('{0}','{1}','{2}','{3}','{4}')",groupid,groupname,grouppid,DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss"),"");
            }
            else
            {
                sql = string.Format("update user_groups set groupname='{0}' where groupid='{1}'", groupname, groupid);
            }

            try
            {
                var cnt=_dapperClient.Execute(sql,null);
                sr.IsSuccess("");
            }
            catch (Exception e)
            {
                sr.IsFailed(e.Message);
            }
            return sr;
        }

        /// <summary>
        /// 删除分组
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPost, Route("delgroup")]

        public ServiceResult delRole([FromBody] RoleRequestDTO role)
        {
            ServiceResult sr = new ServiceResult();

            var groupid = role.Key;
            if (groupid == "" || groupid == null) {
                sr.IsFailed("删除的分组不可为空");
                return sr;
            }
            var sql = string.Format("delete from user_groups where groupid='{0}'",groupid);

            try
            {
                var cnt = _dapperClient.Execute(sql, null);
                sr.IsSuccess("");
            }
            catch (Exception e)
            {
                sr.IsFailed(e.Message);
            }
            return sr;
        }
    }
}
