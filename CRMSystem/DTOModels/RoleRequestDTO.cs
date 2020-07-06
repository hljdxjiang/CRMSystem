using System;
namespace CRMSystem.DTOModels
{
    public class RoleRequestDTO
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 分组编号
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 分组父编号
        /// </summary>
        public string Grouppid { get; set; }

        /// <summary>
        /// 租管理员编号
        /// </summary>
        public string Managerid { get; set; }

    }
}
