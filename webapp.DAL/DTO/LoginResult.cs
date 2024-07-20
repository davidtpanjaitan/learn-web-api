using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapp.DAL.DTO
{
    public class LoginResult
    {
        public LoginResult(string username, string role, string token)
        {
            this.username = username;
            this.role = role;
            this.token = token;
        }

        public string username { get; set; }
        public string role { get; set; }
        public string token { get; set; }
    }
}
