using SE104_Library_Manager.Models;

namespace SE104_Library_Manager.Interfaces;

public interface IAuthService
{
    public Task<UserProfile> AuthenticateAsync(string username, string password);
}
