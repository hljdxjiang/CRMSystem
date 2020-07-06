using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CRMSystem.DTOModels;
using JHC.DataManager;
using JHC.ToolKit.Base;
using JHC.ToolKitExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CRMSystem.Controllers
{
    [Route("api/course")]
    public class CourseController : Controller
    {
        private readonly ILogger<CourseController> _logger;
        private readonly DapperClient _dapperClient;

        public CourseController(ILogger<CourseController> logger, IDapperClientFactory dapperFactory)
        {
            _logger = logger;
            _dapperClient = dapperFactory.GetClient("mysql1");
        }
        // GET: api/values
        [HttpGet, Route("getall")]
        public ServiceResult<List<CourseRequestDTO>> GetCourse()
        {
            ServiceResult<List<CourseRequestDTO>> sr = new ServiceResult<List<CourseRequestDTO>>();
            string sql = "select * from edu_course";
            try
            {

                var list = _dapperClient.Query<CourseRequestDTO>(sql);
                sr.IsSuccess(list);
            }
            catch (Exception e) {
                sr.IsFailed(e.Message);
            }

            return sr;
        }

        [HttpGet, Route("getcourse")]
        public ServiceResult<CourseRequestDTO> GetCourseByID()
        {
            ServiceResult<CourseRequestDTO> sr = new ServiceResult<CourseRequestDTO>();
            var id = HttpContext.Request.Query["courseid"];
            if (string.IsNullOrEmpty(id)) {
                sr.IsFailed("参数错误");
                return sr;
            }

            string sql =string.Format("select * from edu_course where courseid='{0}'",id);
            try
            {
                var list = _dapperClient.QueryFirst<CourseRequestDTO>(sql);
                sr.IsSuccess(list);
            }
            catch (Exception e)
            {
                sr.IsFailed(e.Message);
            }

            return sr;
        }


        /// <summary>
        /// 修改课程状态
        /// </summary>
        /// <param name="cour"></param>
        /// <returns></returns>
        [HttpPost, Route("change")]
        public ServiceResult ChangeCourse([FromBody] CourseRequestDTO cour)
        {
            ServiceResult sr = new ServiceResult();
            var courseid = cour.Courseid;
            var cstatus = cour.Cstatus;
            if (string.IsNullOrEmpty(courseid) || string.IsNullOrEmpty(cstatus)|| !StringExtensions.SqlValidate(courseid)) {
                sr.IsFailed("参数异常");
                return sr;
            }
            string sql = string.Format("update edu_course set cstatus='{0}' where courseid='{1}'", cstatus, courseid);
            try
            {
                var cnt = _dapperClient.Execute(sql, null);
                if (cnt <= 0)
                {
                    sr.IsFailed("更新异常，请刷新后重试");
                }
                else
                {
                    sr.IsSuccess();
                }
            }
            catch (Exception e) {
                sr.IsFailed(e.Message);
            }
            return sr;

        }

        /// <summary>
        /// 修改课程状态
        /// </summary>
        /// <param name="cour"></param>
        /// <returns></returns>
        [HttpPost, Route("delete")]
        public ServiceResult DelCourse([FromBody] CourseRequestDTO cour)
        {
            ServiceResult sr = new ServiceResult();
            var courseid = cour.Courseid;
            if (string.IsNullOrEmpty(courseid)||!StringExtensions.SqlValidate(courseid))
            {
                sr.IsFailed("参数异常");
                return sr;
            }
            string sql = string.Format("delete from  edu_course  where courseid='{0}'", courseid);
            try
            {
                var cnt = _dapperClient.Execute(sql, null);
                if (cnt <= 0)
                {
                    sr.IsFailed("更新异常，请刷新后重试");
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

        /// <summary>
        /// 保存课程信息
        /// </summary>
        /// <param name="cour"></param>
        /// <returns></returns>
        [HttpPost,Route("save")]
        [Authorize]
        public ServiceResult SaveCourse([FromBody] CourseRequestDTO cour)
        {
            ServiceResult sr = new ServiceResult();
            var courseid = cour.Courseid == null || cour.Courseid == "" ?
                _dapperClient.GetSequence("edu_course").ToString().PadLeft(8, '0') : cour.Courseid;
            cour.Courseid = courseid;
            var conn = _dapperClient.Connection;
            conn.Open();
            using (var s = conn.BeginTransaction())
            {
                try
                {

                    using (var cmd = new MySqlCommand())
                    {
                        var sql = string.Format("delete from edu_course where courseid='{0}'", courseid);
                        cmd.CommandText = sql;
                        cmd.Connection = (MySqlConnection)conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new MySqlCommand())
                    {
                        var key = ""; var values = ""; var isfirst = true;
                        foreach (System.Reflection.PropertyInfo info in cour.GetType().GetProperties())
                        {
                            var val = info.GetValue(cour);
                            var name = info.Name;
                            if (string.IsNullOrWhiteSpace((string)val) || val == null)
                            {
                                continue;
                            }
                            //一级标题不做任何处理
                            if (name == "sub1") {
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
                        var sql = string.Format("insert into edu_course({0})values({1})", key, values);
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
                finally
                {
                    conn.Close();
                }
            }
            return sr;

        }
    }
}
