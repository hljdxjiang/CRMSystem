using System;
using System.Collections.Generic;
using System.Data;
using CRMSystem.DTOModels;
using JHC.DataManager;
using JHC.ToolKit.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace CRMSystem.Controllers
{
    [Route("api/subject")]
    [Authorize]
    public class SubjectController : Controller
    {
        private readonly ILogger<SubjectController> _logger;
        private readonly DapperClient _dapperClient;

        public SubjectController(ILogger<SubjectController> logger, IDapperClientFactory dapperFactory)
        {
            _logger = logger;
            _dapperClient = dapperFactory.GetClient("mysql1");
        }

        /// <summary>
        /// 获取所有科目
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("getsubjects")]
        public ServiceResult<List<SubjectDTO>> GetAllSubject()
        {
            ServiceResult<List<SubjectDTO>> sr = new ServiceResult<List<SubjectDTO>>();

            var sql = string.Format("select Subjectid,subjectname,subjectdesc,substatus,Subjectpid from edu_subjects");
            try
            {
                var list = _dapperClient.Query<SubjectDTO>(sql);
                sr.IsSuccess(list);
            }
            catch (Exception e)
            {
                sr.IsFailed(e.Message);
            }
            return sr;
        }

        /// <summary>
        /// 删除科目
        /// </summary>
        /// <param name="subj"></param>
        /// <returns></returns>
        [HttpPost, Route("deletesub")]
        public ServiceResult DeleteSub([FromBody] SubjectDTO subj) {
            ServiceResult sr = new ServiceResult();
            var subid = subj.Subjectid;
            if (subid == null || subid == "") {
                sr.IsFailed("删除信息有误");
                return sr;
            }
            string sql = string.Format("delete from edu_subjects where subjectid='{0}'", subid);
            var cnt = _dapperClient.Execute(sql,null);
            if (cnt <= 0) {
                sr.IsFailed("删除失败，请刷新后重试");
                return sr;
            }
            sr.IsSuccess();
            return sr;
        }

        /// <summary>
        /// 修改科目状态
        /// </summary>
        /// <param name="subj"></param>
        /// <returns></returns>
        [HttpGet, Route("change")]
        public ServiceResult ChangeSub()
        {
            ServiceResult sr = new ServiceResult();
            var subid = HttpContext.Request.Query["subjectid"];
            var status= HttpContext.Request.Query["substatus"];
            if (string.IsNullOrEmpty(subid))
            {
                sr.IsFailed("科目信息有误");
                return sr;
            }
            string sql = string.Format("update  edu_subjects set substatus='{1}' where subjectid='{0}'", subid,status);
            var cnt = _dapperClient.Execute(sql, null);
            if (cnt <= 0)
            {
                sr.IsFailed("删除失败，请刷新后重试");
                return sr;
            }
            sr.IsSuccess();
            return sr;
        }


        /// <summary>
        /// 保存科目
        /// </summary>
        /// <param name="subj"></param>
        /// <returns></returns>
        [HttpPost, Route("save")]
        public ServiceResult SaveSubject([FromBody] SubjectDTO subj) {
            ServiceResult sr = new ServiceResult();
            var subjid = subj.Subjectid==null|| subj.Subjectid == ""?
                _dapperClient.GetSequence("edu_subjects").ToString().PadLeft(8, '0'):subj.Subjectid;
            subj.Subjectid = subjid;
            var conn = _dapperClient.Connection;
            conn.Open();
            using (var s = conn.BeginTransaction()) {
                try
                {

                    using (var cmd = new MySqlCommand())
                    {
                        var sql = string.Format("delete from edu_subjects where subjectid='{0}'", subjid);
                        cmd.CommandText = sql;
                        cmd.Connection = (MySqlConnection)conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new MySqlCommand())
                    {
                        var key = ""; var values = ""; var isfirst = true;
                        foreach (System.Reflection.PropertyInfo info in subj.GetType().GetProperties())
                        {
                            var val = info.GetValue(subj);
                            var name = info.Name;
                            if (string.IsNullOrWhiteSpace((string)val) || val == null)
                            {
                                continue;
                            }
                            if (isfirst)
                            {
                                key += name;
                                values += "'" + val + "'";
                            }
                            else
                            {
                                key += "," + name;
                                values += ",'" + val + "'";
                            }
                            isfirst = false;
                        }
                        var sql = string.Format("insert into edu_subjects({0})values({1})", key, values);
                        cmd.CommandText = sql;
                        cmd.Connection = (MySqlConnection)conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                    s.Commit();
                    sr.IsSuccess();
                }
                catch (Exception e)
                {
                    s.Rollback();
                    sr.IsFailed(e.Message);
                }
                finally {
                    conn.Close();
                }
            }
            return sr;
        }
    }
}
