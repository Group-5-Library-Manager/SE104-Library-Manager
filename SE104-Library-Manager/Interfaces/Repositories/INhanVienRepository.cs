using SE104_Library_Manager.Entities;

namespace SE104_Library_Manager.Interfaces.Repositories;

public interface INhanVienRepository
{
    public Task<List<NhanVien>> GetAllAsync();
    public Task<NhanVien?> GetByIdAsync(int id);

    public Task<bool> ExistsByPhoneNumberAsync(string phoneNumber);

    public Task AddAsync(NhanVien nhanVien, TaiKhoan taiKhoan);
    public Task UpdateAsync(NhanVien nhanVien);
    public Task DeleteAsync(int id);

    public void ValidateNhanVien(NhanVien nhanVien);

    public void ValidateTaiKhoan(TaiKhoan taiKhoan);
}
