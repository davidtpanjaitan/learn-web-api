using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapp.DAL.DTO
{
    public class LoginResult
    {
        public LoginResult(string role, string token)
        {
            this.role = role;
            this.token = token;
        }

        public string role { get; set; }
        public string token { get; set; }
    }
}
