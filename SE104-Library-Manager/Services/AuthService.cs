using BCrypt.Net;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Models;

namespace SE104_Library_Manager.Services;

public class AuthService(ITaiKhoanRepository taiKhoanRepository) : IAuthService
{
    public async Task<UserProfile> AuthenticateAsync(string username, string password)
    {
        TaiKhoan? tk = await taiKhoanRepository.GetByCredentialsAsync(username);

        if (tk == null || !BCrypt.Net.BCrypt.Verify(password, tk.MatKhau))
        {
            throw new UnauthorizedAccessException("Sai tên đăng nhập hoặc mật khẩu");
        }

        UserProfile userProfile = new UserProfile
        {
            MaNhanVien = tk.MaNhanVien,
            TenDangNhap = tk.TenDangNhap,
            TenNhanVien = tk.NhanVien.TenNhanVien,
            DienThoai = tk.NhanVien.DienThoai,
            NgaySinh = tk.NhanVien.NgaySinh,
            Role = tk.VaiTro.TenVaiTro
        };

        return userProfile;
    }
}
