using webapp.DAL.Models;

namespace webapp.DAL.DTO
{
    public class LoginResult
    {
        public LoginResult(User user, string token)
        {
            this.user = user;
            this.token = token;
        }

        public User user { get; set; }
        public string token { get; set; }
    }
}
