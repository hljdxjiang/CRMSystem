using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CRMSystem.DTOModels;
using CRMSystem.Helper;
using JHC.DataManager;
using JHC.ToolKit.Base;
using JHC.ToolKit.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CRMSystem.Controllers
{
    [Route("api/order")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ILogger<OrderController> _logger;
        private readonly DapperClient _dapperClient;
        public OrderController(ILogger<OrderController> logger, IDapperClientFactory dapperFactory)
        {
            _logger = logger;
            _dapperClient = dapperFactory.GetClient("mysql1");
        }
        /// <summary>
        /// 创建工单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost ,Route("save")]
        public ServiceResult SaveOrder([FromBody] OrderRequestDTO order)
        {
            ServiceResult sr = new ServiceResult();

            order.Courseid = string.IsNullOrEmpty(order.Courseid) ?
                _dapperClient.GetSequence("user_order").ToString().PadLeft(8, '0') : order.Courseid;

            bool isfirst = true;
            var key = ""; var values = "";
            foreach (System.Reflection.PropertyInfo info in order.GetType().GetProperties())
            {
                var val = info.GetValue(order);
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
            key += ",crdt";
            values += ",'" + DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss") + "'";
            string sql = string.Format("insert into user_order({0})values({1})", key, values);
            try {
                var cnt = _dapperClient.Execute(sql, null);
                if (cnt <= 0) {
                    sr.IsFailed("保存失败");
                }
                else {
                    sr.IsSuccess();
                }

            } catch (Exception e) {
                sr.IsFailed(e.Message);
            }

            LoggerTimes lt = new LoggerTimes();
            var id = HttpContext.Request.Query["userid"];
            _logger.LogInformation("GetUserByID begin Groupid is<{0}>", id);
            return sr;
        }

        /// <summary>
        /// 查询工单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns
        [HttpPost, Route("query")]
        public ServiceResult<List<OrderRequestDTO>> QueryOrder([FromBody] OrderQueryQequestDTO order) {
            ServiceResult<List<OrderRequestDTO>> sr = new ServiceResult<List<OrderRequestDTO>>();
            HttpContext.Request.Cookies.TryGetValue("_userid", out string userid);
            var isfirst = true;
            var hassub = true;
            var querystr = "";
            if (order == null) {
                sr.IsFailed("参数错误，请重试");
                return sr;
            }

            var subject = order.Subjectid;
            if (string.IsNullOrEmpty(subject)) {
                hassub = false;
            }
            foreach (System.Reflection.PropertyInfo info in order.GetType().GetProperties())
            {
                var s = "";
                var val = (string)info.GetValue(order);
                var name = info.Name;
                if (string.IsNullOrWhiteSpace((string)val) || val == null)
                {
                    continue;
                }
                if (name.ToLower() == "bgdt")
                {
                    s = "crdt>='" + val + "'";
                }
                else if (name.ToLower() == "enddt")
                {
                    s = "crdt<='" + val + "'";
                }
                else if (name.ToLower() == "sub1")
                {
                    if (hassub)
                    {
                        continue;
                    }
                    s = string.Format("subjectid in (select subjectid from edu_subjects where subjectpid='{0}')", val);
                } else if (name.ToLower() == "scope") {
                    if (val == "1")
                    {
                        s = string.Format("userid='{0}'", userid);
                    }
                    else if(val=="2"){
                        s = string.Format("userid in (select userid from user_users where groupid=(select groupid from user_users where userid='{0}' limit 1))", userid);
                    }else {
                        continue;
                    }
                }
                else {
                    s = string.Format("{0}='{1}' ", name, val);
                }

                if (isfirst)
                {
                    querystr += s;
                }
                else
                {
                    querystr +="and "+ s;
                }
                isfirst = false;
            }
            string sql = string.Format("select orderid, stu_userid, prodtype, orgid, getsubjectname(subjectid) subjectid ,getcoursename(courseid) courseid, crdt, ostatus, " +
                "paytype, paydt, ordprice, paycnt, payment, mobile, uname, summary, education, region, getuser(userid) userid, userpid, addr, getsubjectpname(subjectid) sub1," +
                "remark, hteacher from user_order where {0}", querystr);

            //getsubjectname(subjectid),getsubjectpname(subjectid),getuser(userid),getcoursename(courseid)
            try
            {
                var list = _dapperClient.Query<OrderRequestDTO>(sql, null);
                sr.IsSuccess(list);
            }
            catch (Exception e) {
                sr.IsFailed(e.Message);
            }
            return sr;

        }
    }
}
