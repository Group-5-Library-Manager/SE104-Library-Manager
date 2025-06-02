using SE104_Library_Manager.Models;

namespace SE104_Library_Manager.Interfaces;

public interface IUserSessionReader
{
    UserProfile? CurrentUserProfile { get; }
}
