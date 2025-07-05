using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;

namespace SE104_Library_Manager.Repositories;

public class LoaiDocGiaRepository(DatabaseService dbService, IQuyDinhRepository quyDinhRepo) : ILoaiDocGiaRepository
{
    public async Task AddAsync(LoaiDocGia loaiDocGia)
    {
        QuyDinh quyDinh = await quyDinhRepo.GetQuyDinhAsync();
        int count = await dbService.DbContext.DsLoaiDocGia.CountAsync(ldg => !ldg.DaXoa);

        if (count >= quyDinh.SoLoaiDocGiaToiDa)
        {
            throw new InvalidOperationException($"Số lượng loại độc giả đã đạt giới hạn tối đa là {quyDinh.SoLoaiDocGiaToiDa}.");
        }

        if (loaiDocGia == null) throw new ArgumentNullException("Loại độc giả không được là null");
        if (string.IsNullOrWhiteSpace(loaiDocGia.TenLoaiDocGia))
        {
            throw new ArgumentException("Tên loại độc giả không được để trống.");
        }

        var exists = await dbService.DbContext.DsLoaiDocGia.AnyAsync(ldg => ldg.TenLoaiDocGia.ToLower() == loaiDocGia.TenLoaiDocGia.ToLower() && !ldg.DaXoa);

        if (exists)
        {
            throw new InvalidOperationException($"Loại độc giả với tên {loaiDocGia.TenLoaiDocGia} đã tồn tại.");
        }
        
        await dbService.DbContext.DsLoaiDocGia.AddAsync(loaiDocGia);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task DeleteAsync(int id)
    {
        var existingLoaiDocGia = await dbService.DbContext.DsLoaiDocGia.FindAsync(id);

        if (existingLoaiDocGia == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy loại độc giả với mã LDG{id}.");
        }

        if (await dbService.DbContext.DsDocGia.AnyAsync(dg => dg.MaLoaiDocGia == id && !dg.DaXoa))
        {
            throw new InvalidOperationException($"Không thể xóa loại độc giả với mã LDG{id} vì có độc giả đang sử dụng loại này.");
        }

        existingLoaiDocGia.DaXoa = true; // Mark as deleted instead of removing from database

        dbService.DbContext.DsLoaiDocGia.Update(existingLoaiDocGia);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task<List<LoaiDocGia>> GetAllAsync()
    {
        return await dbService.DbContext.DsLoaiDocGia
            .AsNoTracking()
            .Where(ldg => !ldg.DaXoa) // Only get non-deleted LoaiDocGia
            .ToListAsync();
    }

    public Task<LoaiDocGia?> GetByIdAsync(int id)
    {
        return dbService.DbContext.DsLoaiDocGia
            .AsNoTracking()
            .FirstOrDefaultAsync(ldg => ldg.MaLoaiDocGia == id && !ldg.DaXoa);
    }

    public async Task UpdateAsync(LoaiDocGia loaiDocGia)
    {
        if (loaiDocGia == null) throw new ArgumentNullException("Loại độc giả không được là null");
        if (string.IsNullOrWhiteSpace(loaiDocGia.TenLoaiDocGia))
        {
            throw new ArgumentException("Tên loại độc giả không được để trống.");
        }

        if (await dbService.DbContext.DsLoaiDocGia.AnyAsync(ldg => ldg.TenLoaiDocGia.ToLower() == loaiDocGia.TenLoaiDocGia.ToLower() && ldg.MaLoaiDocGia != loaiDocGia.MaLoaiDocGia && !ldg.DaXoa))
        {
            throw new InvalidOperationException($"Loại độc giả với tên {loaiDocGia.TenLoaiDocGia} đã tồn tại.");
        }

        var existingLoaiDocGia = await dbService.DbContext.DsLoaiDocGia.FindAsync(loaiDocGia.MaLoaiDocGia);

        if (existingLoaiDocGia == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy loại độc giả với mã LDG{loaiDocGia.MaLoaiDocGia}.");
        }

        existingLoaiDocGia.TenLoaiDocGia = loaiDocGia.TenLoaiDocGia.Trim();

        dbService.DbContext.DsLoaiDocGia.Update(existingLoaiDocGia);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }
}
