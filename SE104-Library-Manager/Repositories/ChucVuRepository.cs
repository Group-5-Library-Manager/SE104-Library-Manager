using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;

namespace SE104_Library_Manager.Repositories;

public class ChucVuRepository(DatabaseService dbService, IQuyDinhRepository quyDinhRepo, IStaffSessionReader staffSessionReader) : IChucVuRepository
{
    public async Task AddAsync(ChucVu chucVu)
    {
        if (staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        QuyDinh quyDinh = await quyDinhRepo.GetQuyDinhAsync();
        int count = await dbService.DbContext.DsChucVu.CountAsync();
        if (count >= quyDinh.SoChucVuToiDa)
        {
            throw new InvalidOperationException($"Số lượng chức vụ đã đạt giới hạn tối đa là {quyDinh.SoChucVuToiDa}.");
        }

        if (chucVu == null)
        {
            throw new ArgumentNullException("Chức vụ không được là null");
        }

        if (string.IsNullOrWhiteSpace(chucVu.TenChucVu))
        {
            throw new ArgumentException("Tên chức vụ không được để trống.");
        }

        var exists = await dbService.DbContext.DsChucVu.AnyAsync(cv => cv.TenChucVu.ToLower() == chucVu.TenChucVu.ToLower());
        if (exists)
        {
            throw new InvalidOperationException($"Chức vụ với tên {chucVu.TenChucVu} đã tồn tại.");
        }

        await dbService.DbContext.DsChucVu.AddAsync(chucVu);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task DeleteAsync(int id)
    {
        if (staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        var existingChucVu = await dbService.DbContext.DsChucVu.FindAsync(id);

        if (existingChucVu == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy chức vụ với mã CV{id}.");
        }

        if (await dbService.DbContext.DsNhanVien.AnyAsync(nv => nv.MaChucVu == id))
        {
            throw new InvalidOperationException($"Không thể xóa chức vụ với mã CV{id} vì có nhân viên đang sử dụng chức vụ này.");
        }

        dbService.DbContext.DsChucVu.Remove(existingChucVu);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task<List<ChucVu>> GetAllAsync()
    {
        return await dbService.DbContext.DsChucVu
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ChucVu?> GetByIdAsync(int id)
    {
        return await dbService.DbContext.DsChucVu
            .AsNoTracking()
            .FirstOrDefaultAsync(cv => cv.MaChucVu == id);
    }

    public async Task UpdateAsync(ChucVu chucVu)
    {
        if (staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        if (chucVu == null)
        {
            throw new ArgumentNullException("Chức vụ không được là null");
        }

        if (string.IsNullOrWhiteSpace(chucVu.TenChucVu))
        {
            throw new ArgumentException("Tên chức vụ không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(chucVu.TenChucVu))
        {
            throw new ArgumentException("Tên chức vụ không được để trống.");
        }

        var exists = await dbService.DbContext.DsChucVu.AnyAsync(cv => cv.TenChucVu.ToLower() == chucVu.TenChucVu.ToLower() && cv.MaChucVu != chucVu.MaChucVu);
        if (exists)
        {
            throw new InvalidOperationException($"Chức vụ với tên {chucVu.TenChucVu} đã tồn tại.");
        }

        var existingChucVu = await dbService.DbContext.DsChucVu.FindAsync(chucVu.MaChucVu);

        if (existingChucVu == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy chức vụ với mã CV{chucVu.MaChucVu}.");
        }

        existingChucVu.TenChucVu = chucVu.TenChucVu;

        dbService.DbContext.DsChucVu.Update(existingChucVu);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }
}
