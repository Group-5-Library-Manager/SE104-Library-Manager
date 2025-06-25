using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;
using System.Text.RegularExpressions;

namespace SE104_Library_Manager.Repositories;
public class ChiTietPhieuTraRepository(DatabaseService db) : IChiTietPhieuTraRepository
{
    public async Task<List<ChiTietPhieuTra>> GetByPhieuTraAsync(int maPhieuTra)
    {
        return await db.DbContext.DsChiTietPhieuTra
            .AsNoTracking()
            .Include(ct => ct.Sach)
            .Include(ct => ct.PhieuMuon)
            .Where(ct => ct.MaPhieuTra == maPhieuTra && !ct.DaXoa)
            .ToListAsync();
    }

    public async Task<List<ChiTietPhieuTra>> GetAllByDocGiaAsync(int maDocGia)
    {
        return await db.DbContext.DsChiTietPhieuTra
            .Include(ct => ct.PhieuTra)
            .Where(ct => ct.PhieuTra.MaDocGia == maDocGia && !ct.DaXoa)
            .ToListAsync();
    }

    public async Task AddAsync(ChiTietPhieuTra chiTietPhieuTra)
    {
        await db.DbContext.DsChiTietPhieuTra.AddAsync(chiTietPhieuTra);

        // Update book status to available
        var sach = await db.DbContext.DsSach.FindAsync(chiTietPhieuTra.MaSach);
        if (sach != null)
        {
            sach.TrangThai = "Có sẵn";
            db.DbContext.DsSach.Update(sach);
        }

        await db.DbContext.SaveChangesAsync();
        db.DbContext.ChangeTracker.Clear();
    }

    public async Task AddRangeAsync(IEnumerable<ChiTietPhieuTra> dsChiTietPhieuTra)
    {
        await db.DbContext.DsChiTietPhieuTra.AddRangeAsync(dsChiTietPhieuTra);
        await db.DbContext.SaveChangesAsync();
        db.DbContext.ChangeTracker.Clear();
    }

    public async Task DeleteByPhieuTraAsync(int maPhieuTra)
    {
        var dsChiTiet = await db.DbContext.DsChiTietPhieuTra
            .Include(ct => ct.Sach)
            .Where(ct => ct.MaPhieuTra == maPhieuTra)
            .ToListAsync();

        // Cập nhật trạng thái sách về "Đã mượn"
        foreach (var chiTiet in dsChiTiet)
        {
            if (chiTiet.Sach != null)
            {
                chiTiet.Sach.TrangThai = "Đã mượn";
                db.DbContext.DsSach.Update(chiTiet.Sach);
            }
        }

        // Xóa chi tiết phiếu trả
        db.DbContext.DsChiTietPhieuTra.RemoveRange(dsChiTiet);
        await db.DbContext.SaveChangesAsync();
        db.DbContext.ChangeTracker.Clear();
    }
}