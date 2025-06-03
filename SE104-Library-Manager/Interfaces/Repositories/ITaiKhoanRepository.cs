using SE104_Library_Manager.Entities;

namespace SE104_Library_Manager.Interfaces.Repositories;

public interface ITaiKhoanRepository
{
    public Task<TaiKhoan?> GetByCredentialsAsync(string tenDangNhap);
}
