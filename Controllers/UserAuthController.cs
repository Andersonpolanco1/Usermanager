using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using Usermanager.Models;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace Usermanager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private IConfiguration _configuration;
        private IUserService _userService;
        private static User user = new User();

        public UserAuthController(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        [HttpPost, Route("Register")]

        public async Task<ActionResult<User>> Register(UserDTO userDto)
        {
            CreatePasswordHash(userDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.Id = Guid.NewGuid();
            user.UserName = userDto.UserName;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            return Ok(user);
        }

        [HttpPost, Route("Login")]
        public async Task<ActionResult<String>>Login(UserDTO userDto)
        {
            if (user.UserName != userDto.UserName)
                return BadRequest(new { message= $"User {userDto.UserName} not found." });

            if (!IsPasswordCorrect(userDto.Password, user.PasswordSalt, user.PasswordHash)) 
                return BadRequest(new { message="Password is incorrect." });
            
            return Ok(new { token = GetToken() });
        }

        [HttpGet, Route("get-me"), Authorize]
        public ActionResult<Guid> GetMyId()
        {
            return _userService.GetUserId();
        }

        private string GetToken()
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value)
                );

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(claims : GetClaims(), 
                                            expires : DateTime.Now.AddDays(1), 
                                            signingCredentials : credentials
                                            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static List<Claim> GetClaims()
        {
            return new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, "Admin")
            };
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool IsPasswordCorrect(string password, byte[] passwordSalt, byte[] passwordHast)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            return passwordHast.SequenceEqual(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
        }


    }
}
