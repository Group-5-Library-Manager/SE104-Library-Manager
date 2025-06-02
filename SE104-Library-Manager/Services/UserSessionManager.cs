using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Models;

namespace SE104_Library_Manager.Services;

public class UserSessionManager : IUserSessionManager
{
    public UserProfile? CurrentUserProfile { get; private set; }

    public void ClearCurrentUserProfile()
    {
        CurrentUserProfile = null;
    }

    public void SetCurrentUserProfile(UserProfile userProfile)
    {
        if (userProfile == null)
        {
            throw new ArgumentNullException("User profile cannot be null.");
        }

        CurrentUserProfile = userProfile;
    }
}
