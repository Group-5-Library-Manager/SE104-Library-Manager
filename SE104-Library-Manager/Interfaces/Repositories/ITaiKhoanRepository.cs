using SE104_Library_Manager.Entities;

namespace SE104_Library_Manager.Interfaces.Repositories;

public interface ITaiKhoanRepository
{
    public Task<TaiKhoan?> GetByCredentialsAsync(string tenDangNhap);

    public Task<string> GetRoleAsync(int maNhanVien);
    public Task AddAsync(TaiKhoan taiKhoan);
    public Task UpdateUsernameAsync(int maNhanVien, string tenDangNhap);
    public Task UpdatePasswordAsync(int maNhanVien, string matKhauMoi);
    public Task DeleteAsync(int maNhanVien);
}
