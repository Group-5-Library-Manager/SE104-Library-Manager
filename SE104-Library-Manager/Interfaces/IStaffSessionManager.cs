namespace SE104_Library_Manager.Interfaces;

public interface IStaffSessionManager : IStaffSessionReader
{
    void SetCurrentStaffId(int staffId);
    void ClearCurrentStaffId();
}
