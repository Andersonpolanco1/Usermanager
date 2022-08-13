using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using Usermanager.Models;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace Usermanager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private static User user = new User();
        private readonly IConfiguration configuration;

        UserAuthController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost, Route("Register")]

        public async Task<ActionResult<User>> Register(UserDTO userDto)
        {
            CreatePasswordHash(userDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

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
            
            return Ok(GetToken());
        }

        private string GetToken()
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:SecurityToken").Value)
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
                new Claim(ClaimTypes.Name, user.UserName)
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
