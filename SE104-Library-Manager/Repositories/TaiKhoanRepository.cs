using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;

namespace SE104_Library_Manager.Repositories;

public class TaiKhoanRepository(DatabaseService dbService) : ITaiKhoanRepository
{
    public async Task<TaiKhoan?> GetByCredentialsAsync(string tenDangNhap)
    {
        return await dbService.DbContext.DsTaiKhoan
            .Include(tk => tk.NhanVien)
            .Include(tk => tk.VaiTro)
            .FirstOrDefaultAsync(tk => tk.TenDangNhap == tenDangNhap && !tk.DaXoa);
    }
}
