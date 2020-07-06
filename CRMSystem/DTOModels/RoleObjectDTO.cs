using System;
using System.Collections.Generic;

namespace CRMSystem.DTOModels
{
    public struct RoleObjectDTO
    {
        public string roleid { get; set; }
        public string rolename { get; set; }

        public List<string> menus{ get; set; }

        public List<string> auths { get; set; }
    }
}
