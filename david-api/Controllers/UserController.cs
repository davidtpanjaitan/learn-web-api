using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using webapp.DAL.Models;
using webapp.DAL.Repositories;

namespace david_api.Controllers
{
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private UserRepository userRepo;

        public UserController(UserRepository userRepo)
        {
            this.userRepo = userRepo;
        }

        [HttpGet(Name = "getAllUser")]
        public async Task<IActionResult> GetAllUser()
        {
            var res = await userRepo.GetAllAsync();
            return new OkObjectResult(res);
        }

        [HttpPost(Name = "CreateUser")]
        public async Task<IActionResult> Create([FromBody] User user)
        {
            var newuser = await userRepo.CreateWithEncryptedPasswordAsync(user);
            return new OkObjectResult(newuser);
        }

        //login can be done with username or employee id and password

        [HttpPost("login", Name = "LoginUser")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var accept = await userRepo.MatchUserPasswordExist(user);
            return new OkObjectResult(accept ? "Login berhasil, nanti dikasih jwt token tapi belom implemen hehe" : "Login gagal");
        }


        [HttpGet("{id}", Name = "GetUserById")]
        public async Task<IActionResult> Get( [FromRoute] string id)
        {
            User user = await userRepo.GetByIdAsync(id);
            if (user == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(user);
        }

        [HttpPut("{id}", Name = "UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] User updatedUser)
        {
            updatedUser.id = id;
            await userRepo.UpdateAsync(updatedUser);
            return new OkObjectResult(updatedUser);
        }

        [HttpDelete("{id}", Name = "DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            await userRepo.DeleteAsync(id);
            return new OkResult();
        }

    }
}
