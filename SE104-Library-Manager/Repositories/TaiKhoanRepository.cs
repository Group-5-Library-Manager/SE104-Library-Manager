using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;

namespace SE104_Library_Manager.Repositories;

public class TaiKhoanRepository(DatabaseService dbService) : ITaiKhoanRepository
{
    public async Task AddAsync(TaiKhoan taiKhoan)
    {
        var staffSessionReader = App.ServiceProvider?.GetService(typeof(IStaffSessionReader)) as IStaffSessionReader;
        if (staffSessionReader == null)
        {
            throw new InvalidOperationException("Không tìm thấy dịch vụ IStaffSessionReader.");
        }

        if (staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        if (taiKhoan == null)
        {
            throw new ArgumentNullException("Tài khoản không được là null");
        }

        if (string.IsNullOrWhiteSpace(taiKhoan.TenDangNhap))
        {
            throw new ArgumentException("Tên đăng nhập không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(taiKhoan.MatKhau))
        {
            throw new ArgumentException("Mật khẩu không được để trống.");
        }

        if (taiKhoan.MaVaiTro <= 0)
        {
            throw new ArgumentException("Mã vai trò không hợp lệ.");
        }

        if (taiKhoan.MaNhanVien <= 0)
        {
            throw new ArgumentException("Mã nhân viên không hợp lệ.");
        }

        var exists = await dbService.DbContext.DsTaiKhoan.AnyAsync(tk => tk.TenDangNhap.ToLower() == taiKhoan.TenDangNhap.ToLower());
        if (exists)
        {
            throw new InvalidOperationException($"Tài khoản với tên đăng nhập {taiKhoan.TenDangNhap} đã tồn tại.");
        }

        taiKhoan.TenDangNhap = taiKhoan.TenDangNhap.Trim().ToLower();
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(taiKhoan.MatKhau);
        taiKhoan.MatKhau = hashedPassword;

        await dbService.DbContext.DsTaiKhoan.AddAsync(taiKhoan);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task DeleteAsync(int maNhanVien)
    {
        var staffSessionReader = App.ServiceProvider?.GetService(typeof(IStaffSessionReader)) as IStaffSessionReader;
        if (staffSessionReader == null)
        {
            throw new InvalidOperationException("Không tìm thấy dịch vụ IStaffSessionReader.");
        }

        if (staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        var taiKhoan = await dbService.DbContext.DsTaiKhoan.FindAsync(maNhanVien);

        if (taiKhoan == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy tài khoản của nhân viên có mã NV{maNhanVien}.");
        }

        var nhanVien = await dbService.DbContext.DsNhanVien.FindAsync(maNhanVien);
        if (nhanVien == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy nhân viên có mã NV{maNhanVien}.");
        }

        if (!nhanVien.DaXoa)
        {
            throw new InvalidOperationException($"Nhân viên có mã NV{maNhanVien} chưa được xóa. Vui lòng xóa nhân viên trước khi xóa tài khoản.");
        }

        taiKhoan.DaXoa = true;

        dbService.DbContext.DsTaiKhoan.Update(taiKhoan);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task<TaiKhoan?> GetByCredentialsAsync(string tenDangNhap)
    {
        return await dbService.DbContext.DsTaiKhoan
            .Include(tk => tk.NhanVien)
            .Include(tk => tk.VaiTro)
            .FirstOrDefaultAsync(tk => tk.TenDangNhap == tenDangNhap.Trim().ToLower() && !tk.DaXoa);
    }

    public async Task<TaiKhoan?> GetByStaffIdAsync(int maNhanVien)
    {
        var taiKhoan = await dbService.DbContext.DsTaiKhoan
            .Include(tk => tk.NhanVien)
            .Include(tk => tk.VaiTro)
            .FirstOrDefaultAsync(tk => tk.MaNhanVien == maNhanVien && !tk.DaXoa);
        if (taiKhoan == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy tài khoản của nhân viên có mã NV{maNhanVien}.");
        }

        return taiKhoan;
    }

    public async Task<string> GetRoleAsync(int maNhanVien)
    {
        var staffSessionReader = App.ServiceProvider?.GetService(typeof(IStaffSessionReader)) as IStaffSessionReader;
        if (staffSessionReader == null)
        {
            throw new InvalidOperationException("Không tìm thấy dịch vụ IStaffSessionReader.");
        }

        if (staffSessionReader.CurrentStaffId != maNhanVien && staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        var taiKhoan = await dbService.DbContext.DsTaiKhoan
            .AsNoTracking()
            .Include(tk => tk.VaiTro)
            .FirstOrDefaultAsync(tk => tk.MaNhanVien == maNhanVien && !tk.DaXoa);

        if (taiKhoan == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy tài khoản của nhân viên có mã NV{maNhanVien}.");
        }

        return taiKhoan.VaiTro.TenVaiTro;
    }

    public async Task UpdateUsernameAsync(int maNhanVien, string tenDangNhap)
    {
        var staffSessionReader = App.ServiceProvider?.GetService(typeof(IStaffSessionReader)) as IStaffSessionReader;
        if (staffSessionReader == null)
        {
            throw new InvalidOperationException("Không tìm thấy dịch vụ IStaffSessionReader.");
        }

        if (staffSessionReader.CurrentStaffId != maNhanVien && staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        if (maNhanVien <= 0)
        {
            throw new ArgumentException("Mã nhân viên không hợp lệ.");
        }
        if (string.IsNullOrWhiteSpace(tenDangNhap))
        {
            throw new ArgumentException("Tên đăng nhập không được để trống.");
        }

        var existingTaiKhoan = await dbService.DbContext.DsTaiKhoan
            .Include(tk => tk.VaiTro)
            .FirstOrDefaultAsync(tk => tk.MaNhanVien == maNhanVien);

        if (existingTaiKhoan == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy tài khoản của nhân viên có mã NV{maNhanVien}.");
        }

        if (existingTaiKhoan.TenDangNhap.ToLower() != tenDangNhap.ToLower())
        {
            var exists = await dbService.DbContext.DsTaiKhoan.AnyAsync(tk => tk.TenDangNhap.ToLower() == tenDangNhap.ToLower());
            if (exists)
            {
                throw new InvalidOperationException($"Tài khoản với tên đăng nhập {tenDangNhap} đã tồn tại.");
            }
        }

        existingTaiKhoan.TenDangNhap = tenDangNhap.Trim().ToLower();

        dbService.DbContext.DsTaiKhoan.Update(existingTaiKhoan);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task UpdatePasswordAsync(int maNhanVien, string matKhauMoi)
    {
        var staffSessionReader = App.ServiceProvider?.GetService(typeof(IStaffSessionReader)) as IStaffSessionReader;
        if (staffSessionReader == null)
        {
            throw new InvalidOperationException("Không tìm thấy dịch vụ IStaffSessionReader.");
        }

        if (staffSessionReader.CurrentStaffId != maNhanVien && staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        if (string.IsNullOrWhiteSpace(matKhauMoi))
        {
            throw new ArgumentException("Mật khẩu mới không được để trống.");
        }

        if (maNhanVien <= 0)
        {
            throw new ArgumentException("Mã nhân viên không hợp lệ.");
        }

        var taiKhoan = dbService.DbContext.DsTaiKhoan.Find(maNhanVien);

        if (taiKhoan == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy tài khoản của nhân viên có mã NV{maNhanVien}.");
        }

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(matKhauMoi);
        taiKhoan.MatKhau = hashedPassword;

        dbService.DbContext.DsTaiKhoan.Update(taiKhoan);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }
}
