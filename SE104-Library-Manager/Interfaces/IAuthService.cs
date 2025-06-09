namespace SE104_Library_Manager.Interfaces;

public interface IAuthService
{
    public Task<int> AuthenticateAsync(string username, string password);
}
