using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;

namespace SE104_Library_Manager.Repositories;

public class NhanVienRepository(DatabaseService dbService, ITaiKhoanRepository taiKhoanRepo, IStaffSessionReader staffSessionReader) : INhanVienRepository
{
    public async Task AddAsync(NhanVien nhanVien, TaiKhoan taiKhoan)
    {
        if (staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        ValidateNhanVien(nhanVien);
        if (await ExistsByPhoneNumberAsync(nhanVien.DienThoai))
        {
            throw new InvalidOperationException($"Nhân viên với số điện thoại {nhanVien.DienThoai} đã tồn tại.");
        }

        ValidateTaiKhoan(taiKhoan);

        await dbService.DbContext.DsNhanVien.AddAsync(nhanVien);
        await dbService.DbContext.SaveChangesAsync();

        taiKhoan.MaNhanVien = nhanVien.MaNhanVien;
        await taiKhoanRepo.AddAsync(taiKhoan);
        await dbService.DbContext.SaveChangesAsync();

        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task DeleteAsync(int id)
    {
        if (staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        if (staffSessionReader.CurrentStaffId == id)
        {
            throw new InvalidOperationException("Không thể xóa bản thân.");
        }

        var nhanVien = await dbService.DbContext.DsNhanVien.FindAsync(id);

        if (nhanVien == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy nhân viên với mã NV{id}.");
        }

        if (await dbService.DbContext.DsPhieuMuon.AnyAsync(pm => pm.MaNhanVien == id))
        {
            throw new InvalidOperationException($"Không thể xóa nhân viên với mã NV{id} vì có phiếu mượn liên quan đến nhân viên này.");
        }

        nhanVien.DaXoa = true;

        dbService.DbContext.Update(nhanVien);
        await dbService.DbContext.SaveChangesAsync();

        // Delete associated TaiKhoan if exists
        await taiKhoanRepo.DeleteAsync(nhanVien.MaNhanVien);

        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber)
    {
        return await dbService.DbContext.DsNhanVien.AnyAsync(nv => nv.DienThoai == phoneNumber);
    }

    public async Task<List<NhanVien>> GetAllAsync()
    {
        if (staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        return await dbService.DbContext.DsNhanVien
            .AsNoTracking()
            .Include(nv => nv.TaiKhoan)
            .Include(nv => nv.TaiKhoan.VaiTro)
            .Include(nv => nv.ChucVu)
            .Include(nv => nv.BangCap)
            .Include(nv => nv.BoPhan)
            .Where(nv => !nv.DaXoa)
            .ToListAsync();
    }

    public async Task<NhanVien?> GetByIdAsync(int id)
    {
        // Validate if the current staff has permission to view this employee's details
        if (staffSessionReader.CurrentStaffId != id && staffSessionReader.GetCurrentStaffRole() != "Quản trị viên") return null;

        return await dbService.DbContext.DsNhanVien
            .AsNoTracking()
            .Include(nv => nv.TaiKhoan)
            .Include(nv => nv.TaiKhoan.VaiTro)
            .Include(nv => nv.ChucVu)
            .Include(nv => nv.BangCap)
            .Include(nv => nv.BoPhan)
            .FirstOrDefaultAsync(nv => nv.MaNhanVien == id && !nv.DaXoa);
    }

    public async Task UpdateAsync(NhanVien nhanVien)
    {
        if (staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        ValidateNhanVien(nhanVien);

        var existingNhanVien = await dbService.DbContext.DsNhanVien.FindAsync(nhanVien.MaNhanVien);

        if (existingNhanVien == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy nhân viên với mã NV{nhanVien.MaNhanVien}.");
        }

        if (existingNhanVien.DienThoai != nhanVien.DienThoai && await ExistsByPhoneNumberAsync(nhanVien.DienThoai))
        {
            throw new InvalidOperationException($"Nhân viên với số điện thoại {nhanVien.DienThoai} đã tồn tại.");
        }

        existingNhanVien.TenNhanVien = nhanVien.TenNhanVien;
        existingNhanVien.DiaChi = nhanVien.DiaChi;
        existingNhanVien.DienThoai = nhanVien.DienThoai;
        existingNhanVien.NgaySinh = nhanVien.NgaySinh;
        existingNhanVien.MaChucVu = nhanVien.MaChucVu;
        existingNhanVien.MaBangCap = nhanVien.MaBangCap;
        existingNhanVien.MaBoPhan = nhanVien.MaBoPhan;

        dbService.DbContext.Update(existingNhanVien);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }

    public void ValidateNhanVien(NhanVien nhanVien)
    {
        if (nhanVien == null)
        {
            throw new ArgumentNullException("Nhân viên không được là null");
        }

        if (nhanVien.TenNhanVien == string.Empty)
        {
            throw new ArgumentException("Tên nhân viên không được để trống.");
        }

        if (nhanVien.DienThoai == string.Empty)
        {
            throw new ArgumentException("Số điện thoại không được để trống.");
        }
        if (nhanVien.DienThoai.Length != 10)
        {
            throw new ArgumentException("Số điện thoại phải có 10 chữ số.");
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(nhanVien.DienThoai, @"^\d{10}$"))
        {
            throw new ArgumentException("Số điện thoại chỉ được chứa các chữ số.");
        }

        if (nhanVien.NgaySinh == default)
        {
            throw new ArgumentException("Ngày sinh không được để trống.");
        }

        if (nhanVien.DiaChi == string.Empty)
        {
            throw new ArgumentException("Địa chỉ không được để trống.");
        }

        var age = DateTime.Now.Year - nhanVien.NgaySinh.Year;
        // If the birthday hasn't occurred yet this year, subtract one from the age
        if (DateOnly.FromDateTime(DateTime.Now) < nhanVien.NgaySinh.AddYears(age))
        {
            age--;
        }

        if (age < 18 || age > 60)
        {
            throw new ArgumentException("Tuổi nhân viên phải từ 18 đến 60.");
        }

        if (nhanVien.MaChucVu <= 0)
        {
            throw new ArgumentException("Mã chức vụ không hợp lệ.");
        }

        if (nhanVien.MaBangCap <= 0)
        {
            throw new ArgumentException("Mã bằng cấp không hợp lệ.");
        }

        if (nhanVien.MaBoPhan <= 0)
        {
            throw new ArgumentException("Mã bộ phận không hợp lệ.");
        }
    }

    public void ValidateTaiKhoan(TaiKhoan taiKhoan)
    {
        if (taiKhoan == null)
        {
            throw new ArgumentNullException("Tài khoản không được là null");
        }
        if (string.IsNullOrWhiteSpace(taiKhoan.TenDangNhap))
        {
            throw new ArgumentException("Tên đăng nhập không được để trống.");
        }

        var exists = dbService.DbContext.DsTaiKhoan.AnyAsync(tk => tk.TenDangNhap.ToLower() == taiKhoan.TenDangNhap.ToLower()).Result;
        if (exists)
        {
            throw new InvalidOperationException($"Tài khoản với tên đăng nhập {taiKhoan.TenDangNhap} đã tồn tại.");
        }

        if (string.IsNullOrWhiteSpace(taiKhoan.MatKhau))
        {
            throw new ArgumentException("Mật khẩu không được để trống.");
        }
        if (taiKhoan.MaVaiTro <= 0)
        {
            throw new ArgumentException("Mã vai trò không hợp lệ.");
        }
        if (taiKhoan.MaNhanVien < 0)
        {
            throw new ArgumentException("Mã nhân viên không hợp lệ.");
        }
    }
}
