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

        // Update book status and quantity
        var sach = await db.DbContext.DsSach.FindAsync(chiTietPhieuTra.MaSach);
        if (sach != null)
        {
            sach.SoLuongHienCo += chiTietPhieuTra.SoLuongTra;
            SE104_Library_Manager.Repositories.SachRepository.UpdateBookStatus(sach);
            db.DbContext.DsSach.Update(sach);
        }

        await db.DbContext.SaveChangesAsync();
        db.DbContext.ChangeTracker.Clear();
    }

    public async Task AddRangeAsync(IEnumerable<ChiTietPhieuTra> dsChiTietPhieuTra)
    {
        await db.DbContext.DsChiTietPhieuTra.AddRangeAsync(dsChiTietPhieuTra);

        var maSachList = dsChiTietPhieuTra.Select(c => c.MaSach).Distinct().ToList();
        var sachList = await db.DbContext.DsSach.Where(s => maSachList.Contains(s.MaSach)).ToListAsync();

        foreach (var sach in sachList)
        {
            // Sum all SoLuongTra for this book
            var totalReturned = dsChiTietPhieuTra.Where(c => c.MaSach == sach.MaSach).Sum(c => c.SoLuongTra);
            sach.SoLuongHienCo += totalReturned;
            SE104_Library_Manager.Repositories.SachRepository.UpdateBookStatus(sach);
        }

        await db.DbContext.SaveChangesAsync();
        db.DbContext.ChangeTracker.Clear();
    }


    public async Task DeleteByPhieuTraAsync(int maPhieuTra)
    {
        var dsChiTiet = await db.DbContext.DsChiTietPhieuTra
            .Include(ct => ct.Sach)
            .Where(ct => ct.MaPhieuTra == maPhieuTra)
            .ToListAsync();

        // Cập nhật trạng thái sách
        foreach (var chiTiet in dsChiTiet)
        {
            if (chiTiet.Sach != null)
            {
                // Decrease SoLuongHienCo since we're deleting the return record
                chiTiet.Sach.SoLuongHienCo -= chiTiet.SoLuongTra;
                SE104_Library_Manager.Repositories.SachRepository.UpdateBookStatus(chiTiet.Sach);
                db.DbContext.DsSach.Update(chiTiet.Sach);
            }
        }

        // Xóa chi tiết phiếu trả
        db.DbContext.DsChiTietPhieuTra.RemoveRange(dsChiTiet);
        await db.DbContext.SaveChangesAsync();
        db.DbContext.ChangeTracker.Clear();
    }
}