using SE104_Library_Manager.Models;

namespace SE104_Library_Manager.Interfaces;

public interface IUserSessionManager : IUserSessionReader
{
    void SetCurrentUserProfile(UserProfile userProfile);
    void ClearCurrentUserProfile();
}
