namespace Usermanager.Models
{
    public interface IUserService
    {
        Guid GetUserId();
        string GetUserName();
    }
}
