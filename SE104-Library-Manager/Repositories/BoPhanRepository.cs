using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;

namespace SE104_Library_Manager.Repositories;

public class BoPhanRepository(DatabaseService dbService, IQuyDinhRepository quyDinhRepo, IStaffSessionReader staffSessionReader) : IBoPhanRepository
{
    public async Task AddAsync(BoPhan boPhan)
    {
        if (staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        QuyDinh quyDinh = await quyDinhRepo.GetQuyDinhAsync();
        int count = await dbService.DbContext.DsBoPhan.CountAsync(bp => !bp.DaXoa);
        if (count >= quyDinh.SoBoPhanToiDa)
        {
            throw new InvalidOperationException($"Số lượng bộ phận đã đạt giới hạn tối đa là {quyDinh.SoBoPhanToiDa}.");
        }

        if (boPhan == null)
        {
            throw new ArgumentNullException("Bộ phận không được là null");
        }

        if (string.IsNullOrWhiteSpace(boPhan.TenBoPhan))
        {
            throw new ArgumentException("Tên bộ phận không được để trống.");
        }

        boPhan.TenBoPhan = boPhan.TenBoPhan.Trim();

        var exists = await dbService.DbContext.DsBoPhan.AnyAsync(bp => bp.TenBoPhan.ToLower() == boPhan.TenBoPhan.ToLower() && !bp.DaXoa);
        if (exists)
        {
            throw new InvalidOperationException($"Bộ phận với tên {boPhan.TenBoPhan} đã tồn tại.");
        }

        await dbService.DbContext.DsBoPhan.AddAsync(boPhan);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task DeleteAsync(int id)
    {
        if (staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        var existingBoPhan = await dbService.DbContext.DsBoPhan.FindAsync(id);
        if (existingBoPhan == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bộ phận với mã BP{id}.");
        }

        if (await dbService.DbContext.DsNhanVien.AnyAsync(nv => nv.MaBoPhan == id && !nv.DaXoa))
        {
            throw new InvalidOperationException($"Không thể xóa bộ phận với mã BP{id} vì có nhân viên đang sử dụng bộ phận này.");
        }

        existingBoPhan.DaXoa = true;

        dbService.DbContext.DsBoPhan.Update(existingBoPhan);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task<List<BoPhan>> GetAllAsync()
    {
        return await dbService.DbContext.DsBoPhan
            .AsNoTracking()
            .Where(bp => !bp.DaXoa) // Only include non-deleted BoPhan records
            .ToListAsync();
    }

    public async Task<BoPhan?> GetByIdAsync(int id)
    {
        return await dbService.DbContext.DsBoPhan
            .AsNoTracking()
            .FirstOrDefaultAsync(bp => bp.MaBoPhan == id && !bp.DaXoa);
    }

    public async Task UpdateAsync(BoPhan boPhan)
    {
        if (staffSessionReader.GetCurrentStaffRole() != "Quản trị viên")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        if (boPhan == null)
        {
            throw new ArgumentNullException("Bộ phận không được là null");
        }

        if (string.IsNullOrWhiteSpace(boPhan.TenBoPhan))
        {
            throw new ArgumentException("Tên bộ phận không được để trống.");
        }

        var exists = await dbService.DbContext.DsBoPhan.AnyAsync(bp => bp.TenBoPhan.ToLower() == boPhan.TenBoPhan.ToLower() && bp.MaBoPhan != boPhan.MaBoPhan && !bp.DaXoa);
        if (exists)
        {
            throw new InvalidOperationException($"Bộ phận với tên {boPhan.TenBoPhan} đã tồn tại.");
        }

        var existingBoPhan = await dbService.DbContext.DsBoPhan.FindAsync(boPhan.MaBoPhan);
        if (existingBoPhan == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bộ phận với mã BP{boPhan.MaBoPhan}.");
        }

        existingBoPhan.TenBoPhan = boPhan.TenBoPhan.Trim();

        dbService.DbContext.DsBoPhan.Update(existingBoPhan);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }
}
