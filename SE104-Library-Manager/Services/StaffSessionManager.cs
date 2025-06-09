using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Interfaces.Repositories;

namespace SE104_Library_Manager.Services;

public class StaffSessionManager(ITaiKhoanRepository taiKhoanRepository) : IStaffSessionManager
{
    public int CurrentStaffId { get; private set; }

    public void ClearCurrentStaffId()
    {
        CurrentStaffId = 0;
    }

    public string GetCurrentStaffRole()
    {
        return taiKhoanRepository.GetRoleAsync(CurrentStaffId).GetAwaiter().GetResult() ?? "Unknown";
    }

    public void SetCurrentStaffId(int staffId)
    {
        if (staffId <= 0)
        {
            throw new ArgumentNullException("Invalid staff id ");
        }

        CurrentStaffId = staffId;
    }
}
