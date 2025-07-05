using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;

namespace SE104_Library_Manager.Repositories;

public class BangCapRepository(DatabaseService dbService, IQuyDinhRepository quyDinhRepo, IStaffSessionReader staffSessionReader) : IBangCapRepository
{
    public async Task AddAsync(BangCap bangCap)
    {
        if (staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        QuyDinh quyDinh = await quyDinhRepo.GetQuyDinhAsync();
        int count = await dbService.DbContext.DsBangCap.CountAsync(bc => !bc.DaXoa); // Count only non-deleted BangCap records
        if (count >= quyDinh.SoBangCapToiDa)
        {
            throw new InvalidOperationException($"Số lượng bằng cấp đã đạt giới hạn tối đa là {quyDinh.SoBangCapToiDa}.");
        }

        if (bangCap == null)
        {
            throw new ArgumentNullException("Bằng cấp không được là null");
        }

        if (string.IsNullOrWhiteSpace(bangCap.TenBangCap))
        {
            throw new ArgumentException("Tên bằng cấp không được để trống.");
        }

        bangCap.TenBangCap = bangCap.TenBangCap.Trim();

        var exists = await dbService.DbContext.DsBangCap.AnyAsync(bc => bc.TenBangCap.ToLower() == bangCap.TenBangCap.ToLower() && !bc.DaXoa);
        if (exists)
        {
            throw new InvalidOperationException($"Bằng cấp với tên {bangCap.TenBangCap} đã tồn tại.");
        }

        await dbService.DbContext.AddAsync(bangCap);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task DeleteAsync(int id)
    {
        if (staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        var existingBangCap = await dbService.DbContext.DsBangCap.FindAsync(id);

        if (existingBangCap == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bằng cấp với mã BC{id}.");
        }

        if (await dbService.DbContext.DsNhanVien.AnyAsync(nv => nv.MaBangCap == id && !nv.DaXoa))
        {
            throw new InvalidOperationException($"Không thể xóa bằng cấp với mã BC{id} vì có nhân viên đang sử dụng bằng cấp này.");
        }

        existingBangCap.DaXoa = true;

        dbService.DbContext.DsBangCap.Update(existingBangCap);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task<List<BangCap>> GetAllAsync()
    {
        return await dbService.DbContext.DsBangCap
            .AsNoTracking()
            .Where(bc => !bc.DaXoa) // Only include non-deleted BangCap records
            .ToListAsync();
    }

    public async Task<BangCap?> GetByIdAsync(int id)
    {
        return await dbService.DbContext.DsBangCap
            .AsNoTracking()
            .FirstOrDefaultAsync(bc => bc.MaBangCap == id && !bc.DaXoa);
    }

    public async Task UpdateAsync(BangCap bangCap)
    {
        if (staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        if (bangCap == null)
        {
            throw new ArgumentNullException("Bằng cấp không được là null");
        }

        if (string.IsNullOrWhiteSpace(bangCap.TenBangCap))
        {
            throw new ArgumentException("Tên bằng cấp không được để trống.");
        }

        var exists = dbService.DbContext.DsBangCap.Any(bc => bc.TenBangCap.ToLower() == bangCap.TenBangCap.ToLower() && bc.MaBangCap != bangCap.MaBangCap && !bc.DaXoa);
        if (exists)
        {
            throw new InvalidOperationException($"Bằng cấp với tên {bangCap.TenBangCap} đã tồn tại.");
        }

        var existingBangCap = await dbService.DbContext.DsBangCap.FindAsync(bangCap.MaBangCap);
        if (existingBangCap == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bằng cấp với mã BC{bangCap.MaBangCap}.");
        }

        existingBangCap.TenBangCap = bangCap.TenBangCap.Trim();

        dbService.DbContext.DsBangCap.Update(existingBangCap);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }
}
