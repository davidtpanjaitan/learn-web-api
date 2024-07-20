using AutoMapper;
using david_api.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using webapp.DAL.DTO;
using webapp.DAL.Enum;
using webapp.DAL.Models;
using webapp.DAL.Repositories;
using webapp.DAL.Tools;
using User = webapp.DAL.Models.User;

namespace david_api.Controllers
{
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private UserRepository userRepo;
        private IMapper mapper;
        string key;
        string issuer;
        string audience;

        public UserController(UserRepository userRepo)
        {
            this.userRepo = userRepo;
            var configBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            var config = configBuilder.Build();

            key = config["jwtkey"];
            issuer = config["jwtissuer"];
            audience = config["jwtaudience"];
            var mappingconfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserDTO, User>();
            });
            mapper = mappingconfig.CreateMapper();

        }

        [HttpGet(Name = "getAllUser")]
        public async Task<IActionResult> GetAllUser()
        {
            var res = await userRepo.GetAllAsync();
            return new OkObjectResult(res);
        }

        [Authorize(Roles = "admin")]
        [HttpPost(Name = "CreateUser")]
        public async Task<IActionResult> Create([FromBody] UserDTO user)
        {
            try
            {
                if (!Enum.TryParse(typeof(Roles), user.role, out _))
                {
                    return new BadRequestObjectResult("role must be one of [admin, petugasLokasi, picLokasi, petugasWarehouse]");
                }
                var newuser = mapper.Map<User>(user);
                await userRepo.CreateWithEncryptedPasswordAsync(newuser);
                return new OkObjectResult($"user created {newuser.username}");
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return new ConflictObjectResult("Username not unique");
            }
        }

        //login can be done with username or employee id and password
        [HttpPost("login", Name = "LoginUser")]
        public async Task<IActionResult> Login([FromBody] UserDTO user)
        {
            var loginDetail = mapper.Map<User>(user);
            var role = await userRepo.MatchUserPasswordExist(loginDetail);
            if (role != "")
            {
                var token = TokenService.BuildToken(key, issuer, audience, user.username, role);
                return new OkObjectResult(new LoginResult(user.username, role, token));
            } 
            else
            {
                return new UnauthorizedObjectResult("Login gagal");
            }
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
        public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] UserDTO updatedUser)
        {
            try {
            var user = mapper.Map<User>(updatedUser);
            user.id = id;
            user = await userRepo.UpdateAsync(user);
            return new OkObjectResult(user);
        } 
            catch (CosmosException ex) when(ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new Exception("Username not unique");
        }
    }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}", Name = "DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            await userRepo.DeleteAsync(id);
            return new OkResult();
        }

        [Authorize(Roles="admin")]
        [HttpGet("parse-jwt")]
        public async Task<IActionResult> parse()
        {
            var claims = this.User.Claims;
            var dict = new Dictionary<string, string>();
            foreach (var c in claims)
            {
                dict.Add(c.Type, c.Value);
            }
            return new OkObjectResult(dict);
        }
    }
}
