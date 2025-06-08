namespace SE104_Library_Manager.Interfaces;

public interface IStaffSessionReader
{
    int CurrentStaffId { get; }

    public string GetCurrentStaffRole();
}
