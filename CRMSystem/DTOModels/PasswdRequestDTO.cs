using System;
namespace CRMSystem.DTOModels
{
    public class PasswdRequestDTO
    {
        public string Userid{ get; set; }
        public string OrgPasswd { get; set; }
        public string NewPasswd { get; set; }
    }
}
