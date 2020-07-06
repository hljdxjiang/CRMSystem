using System;
using System.Collections.Generic;

namespace CRMSystem.DTOModels
{
    public class LoginUser
    {
        public string Userid{get;set;}

        public string Token { get; set; }

        public string RefToken { get; set; }

        public string RoleID { get; set; }

        public string Roles { get; set; }

        public string RefreshTime { get; set;}

    }
}
