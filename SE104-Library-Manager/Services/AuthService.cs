using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Interfaces.Repositories;

namespace SE104_Library_Manager.Services;

public class AuthService(ITaiKhoanRepository taiKhoanRepository) : IAuthService
{
    public async Task<int> AuthenticateAsync(string username, string password)
    {
        TaiKhoan? tk = await taiKhoanRepository.GetByCredentialsAsync(username);

        if (tk == null || !BCrypt.Net.BCrypt.Verify(password, tk.MatKhau))
        {
            throw new UnauthorizedAccessException("Sai tên đăng nhập hoặc mật khẩu");
        }

        return tk.MaNhanVien;
    }
}
