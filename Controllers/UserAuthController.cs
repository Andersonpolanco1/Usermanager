using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Usermanager.Models;

namespace Usermanager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        public static User user = new User();

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
            return Ok("Nice!!!");
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool IsPasswordCorrect(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                return user.PasswordHash == hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }


    }
}
