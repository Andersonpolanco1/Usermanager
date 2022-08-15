using System.Security.Claims;
using Usermanager.Models;

namespace Usermanager.Services
{
    public class UserService : IUserService
    {
        private IHttpContextAccessor _contextAccesor { get; set; }

        public UserService(IHttpContextAccessor contextAccessor)
        {
            _contextAccesor = contextAccessor;
        }

        public Guid GetUserId()
        {
            if(_contextAccesor.HttpContext != null)
                return Guid.Parse(_contextAccesor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Guid.Parse(string.Empty);
        }

        public string GetUserName()
        {
            return _contextAccesor.HttpContext == null ? string.Empty : _contextAccesor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
        }

        

    }
}
