using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;
using System.Text.RegularExpressions;
using System.Linq;

namespace SE104_Library_Manager.Repositories;
public class ChiTietPhieuTraRepository(DatabaseService db) : IChiTietPhieuTraRepository
{
    public async Task<List<ChiTietPhieuTra>> GetByPhieuTraAsync(int maPhieuTra)
    {
        return await db.DbContext.DsChiTietPhieuTra
            .AsNoTracking()
            .Include(ct => ct.BanSaoSach)
                .ThenInclude(bs => bs.Sach)
            .Include(ct => ct.PhieuMuon)
            .Where(ct => ct.MaPhieuTra == maPhieuTra && !ct.DaXoa)
            .ToListAsync();
    }

    public async Task<List<ChiTietPhieuTra>> GetAllByDocGiaAsync(int maDocGia)
    {
        return await db.DbContext.DsChiTietPhieuTra
            .Include(ct => ct.PhieuTra)
            .Include(ct => ct.BanSaoSach)
                .ThenInclude(bs => bs.Sach)
            .Where(ct => ct.PhieuTra.MaDocGia == maDocGia && !ct.DaXoa)
            .ToListAsync();
    }

    public async Task AddAsync(ChiTietPhieuTra chiTietPhieuTra)
    {
        // Đảm bảo MaPhieuTra đã được set
        if (chiTietPhieuTra.MaPhieuTra == 0)
        {
            throw new InvalidOperationException("MaPhieuTra must be set before adding ChiTietPhieuTra");
        }

        await db.DbContext.DsChiTietPhieuTra.AddAsync(chiTietPhieuTra);

        // Update copy status and book quantity
        var banSao = await db.DbContext.DsBanSaoSach
            .Include(bs => bs.Sach)
            .FirstOrDefaultAsync(bs => bs.MaBanSao == chiTietPhieuTra.MaBanSao);
        if (banSao != null)
        {
            banSao.TinhTrang = "Có sẵn";
            db.DbContext.DsBanSaoSach.Update(banSao);
            
            var sach = await db.DbContext.DsSach.FindAsync(banSao.MaSach);
            if (sach != null)
            {
                sach.SoLuongHienCo += 1;
                SE104_Library_Manager.Repositories.SachRepository.UpdateBookStatus(sach);
                db.DbContext.DsSach.Update(sach);
            }
        }

        await db.DbContext.SaveChangesAsync();
        db.DbContext.ChangeTracker.Clear();
    }

    public async Task AddRangeAsync(IEnumerable<ChiTietPhieuTra> dsChiTietPhieuTra)
    {
        // Đảm bảo tất cả MaPhieuTra đã được set
        foreach (var ct in dsChiTietPhieuTra)
        {
            if (ct.MaPhieuTra == 0)
            {
                throw new InvalidOperationException("MaPhieuTra must be set before adding ChiTietPhieuTra");
            }
        }

        await db.DbContext.DsChiTietPhieuTra.AddRangeAsync(dsChiTietPhieuTra);

        // Get all unique book IDs from the returned copies
        var maSachList = dsChiTietPhieuTra
            .Select(ct => ct.BanSaoSach.MaSach)
            .Distinct()
            .ToList();
        
        var sachList = await db.DbContext.DsSach
            .Where(s => maSachList.Contains(s.MaSach))
            .ToListAsync();

        // Update book quantities
        foreach (var sach in sachList)
        {
            var returnedCopiesCount = dsChiTietPhieuTra
                .Count(ct => ct.BanSaoSach.MaSach == sach.MaSach);
            sach.SoLuongHienCo += returnedCopiesCount;
            SE104_Library_Manager.Repositories.SachRepository.UpdateBookStatus(sach);
        }

        // Update copy statuses
        var maBanSaoList = dsChiTietPhieuTra.Select(ct => ct.MaBanSao).ToList();
        var banSaoList = await db.DbContext.DsBanSaoSach
            .Include(bs => bs.Sach)
            .Where(bs => maBanSaoList.Contains(bs.MaBanSao))
            .ToListAsync();

        foreach (var banSao in banSaoList)
        {
            banSao.TinhTrang = "Có sẵn";
        }

        await db.DbContext.SaveChangesAsync();
        db.DbContext.ChangeTracker.Clear();
    }

    public async Task DeleteByPhieuTraAsync(int maPhieuTra)
    {
        var dsChiTiet = await db.DbContext.DsChiTietPhieuTra
            .Include(ct => ct.BanSaoSach)
                .ThenInclude(bs => bs.Sach)
            .Where(ct => ct.MaPhieuTra == maPhieuTra)
            .ToListAsync();

        // Update copy and book status
        foreach (var chiTiet in dsChiTiet)
        {
            if (chiTiet.BanSaoSach != null)
            {
                // Restore copy status to borrowed
                chiTiet.BanSaoSach.TinhTrang = "Đã mượn";
                db.DbContext.DsBanSaoSach.Update(chiTiet.BanSaoSach);
                
                // Decrease book quantity since we're deleting the return record
                if (chiTiet.BanSaoSach.Sach != null)
                {
                    chiTiet.BanSaoSach.Sach.SoLuongHienCo -= 1;
                    SE104_Library_Manager.Repositories.SachRepository.UpdateBookStatus(chiTiet.BanSaoSach.Sach);
                    db.DbContext.DsSach.Update(chiTiet.BanSaoSach.Sach);
                }
            }
        }

        // Delete return details
        db.DbContext.DsChiTietPhieuTra.RemoveRange(dsChiTiet);
        await db.DbContext.SaveChangesAsync();
        db.DbContext.ChangeTracker.Clear();
    }

    public async Task<bool> HasCopiesBeenBorrowedAgainAsync(int maPhieuTra)
    {
        // Lấy danh sách bản sao trong phiếu trả
        var banSaoTrongPhieuTra = await db.DbContext.DsChiTietPhieuTra
            .Where(ct => ct.MaPhieuTra == maPhieuTra)
            .Select(ct => ct.MaBanSao)
            .ToListAsync();

        if (!banSaoTrongPhieuTra.Any())
            return false;

        // Lấy ngày trả của phiếu trả
        var ngayTra = await db.DbContext.DsPhieuTra
            .Where(pt => pt.MaPhieuTra == maPhieuTra)
            .Select(pt => pt.NgayTra)
            .FirstOrDefaultAsync();

        if (ngayTra == default)
            return false;

        // Kiểm tra xem có bản sao nào đã được mượn lại sau khi trả không
        var hasBeenBorrowedAgain = await db.DbContext.DsChiTietPhieuMuon
            .AnyAsync(ct => banSaoTrongPhieuTra.Contains(ct.MaBanSao) && 
                           !ct.PhieuMuon.DaXoa &&
                           ct.PhieuMuon.NgayMuon >= ngayTra);

        return hasBeenBorrowedAgain;
    }
}