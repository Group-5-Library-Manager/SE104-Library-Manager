using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;
using SE104_Library_Manager.ViewModels.Return;
using System.Linq;

namespace SE104_Library_Manager.Repositories;

public class PhieuTraRepository(DatabaseService db) : IPhieuTraRepository
{
    public async Task<List<PhieuTra>> GetAllAsync()
    {
        return await db.DbContext.DsPhieuTra
            .AsNoTracking()
            .Include(p => p.DsChiTietPhieuTra)
                .ThenInclude(ct => ct.BanSaoSach)
                    .ThenInclude(bs => bs.Sach)
            .Include(p => p.DsChiTietPhieuTra)
                .ThenInclude(ct => ct.PhieuMuon)
            .Include(p => p.DocGia)
            .Include(p => p.NhanVien)
            .Where(p => !p.DaXoa)
            .ToListAsync();
    }

    public async Task<PhieuTra?> GetByIdAsync(int maPhieuTra)
    {
        return await db.DbContext.DsPhieuTra
            .Include(p => p.DocGia)
            .Include(p => p.NhanVien)
            .Include(p => p.DsChiTietPhieuTra)
                .ThenInclude(ct => ct.BanSaoSach)
                    .ThenInclude(bs => bs.Sach)
            .Include(p => p.DsChiTietPhieuTra)
                .ThenInclude(ct => ct.PhieuMuon)
            .FirstOrDefaultAsync(p => p.MaPhieuTra == maPhieuTra && !p.DaXoa);
    }

    public async Task<List<DocGia>> GetDocGiaDangCoSachMuonAsync()
    {
        var docGias = await db.DbContext.DsChiTietPhieuMuon
            .Include(ct => ct.PhieuMuon)
                .ThenInclude(pm => pm.DocGia)
            .Where(ct =>
                !ct.PhieuMuon.DaXoa &&
                !ct.PhieuMuon.DocGia.DaXoa &&
                !db.DbContext.DsChiTietPhieuTra.Any(tr =>
                    tr.MaPhieuMuon == ct.MaPhieuMuon && tr.MaBanSao == ct.MaBanSao))
            .Select(ct => ct.PhieuMuon.DocGia) 
            .Distinct()
            .ToListAsync();

        return docGias;
    }

    public async Task<List<ChiTietPhieuMuon>> GetBanSaoDangMuonByDocGiaAsync(int maDocGia)
    {
        var docGia = await db.DbContext.DsDocGia.FindAsync(maDocGia);
        if (docGia == null)
            throw new KeyNotFoundException($"Không tìm thấy độc giả với mã DG{maDocGia}.");
        return await db.DbContext.DsChiTietPhieuMuon
            .Include(ct => ct.BanSaoSach)
                .ThenInclude(bs => bs.Sach)
            .Include(ct => ct.PhieuMuon)
            .Where(ct => ct.PhieuMuon.MaDocGia == maDocGia && !ct.PhieuMuon.DaXoa)
            .Where(ct => !db.DbContext.DsChiTietPhieuTra.Any(tr => tr.MaPhieuMuon == ct.MaPhieuMuon && tr.MaBanSao == ct.MaBanSao))
            .ToListAsync();
    }

    public async Task<ChiTietPhieuMuon?> GetChiTietMuonMoiNhatChuaTraAsync(int maBanSao)
    {
        return await db.DbContext.DsChiTietPhieuMuon
            .Include(ct => ct.PhieuMuon)
            .Include(ct => ct.BanSaoSach)
                .ThenInclude(bs => bs.Sach)
            .Where(ct => ct.MaBanSao == maBanSao && !ct.PhieuMuon.DaXoa)
            .Where(ct => !db.DbContext.DsChiTietPhieuTra.Any(tr => tr.MaPhieuMuon == ct.MaPhieuMuon && tr.MaBanSao == maBanSao))
            .OrderByDescending(ct => ct.PhieuMuon.NgayMuon)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(PhieuTra phieuTra, List<ChiTietPhieuTraInfo> chiTietBanSao)
    {
        await db.DbContext.DsPhieuTra.AddAsync(phieuTra);
        await db.DbContext.SaveChangesAsync();
        
        var newChiTietList = new List<ChiTietPhieuTra>();
        foreach (var ct in chiTietBanSao)
        {
            var newChiTiet = new ChiTietPhieuTra
            {
                MaPhieuTra = phieuTra.MaPhieuTra,
                MaPhieuMuon = ct.MaPhieuMuon,
                MaBanSao = ct.MaBanSao,
                TienPhat = ct.TienPhat
            };
            newChiTietList.Add(newChiTiet);
            
            // Update copy status
            var banSao = await db.DbContext.DsBanSaoSach
                .Include(bs => bs.Sach)
                .FirstOrDefaultAsync(bs => bs.MaBanSao == ct.MaBanSao);
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
        }
        
        await db.DbContext.DsChiTietPhieuTra.AddRangeAsync(newChiTietList);
        await db.DbContext.SaveChangesAsync();
        db.DbContext.ChangeTracker.Clear();
    }

    public async Task UpdateAsync(PhieuTra phieuTra, List<ChiTietPhieuTraInfo> chiTietBanSao)
    {
        var existing = await db.DbContext.DsPhieuTra.FindAsync(phieuTra.MaPhieuTra);
        if (existing == null)
            throw new Exception($"Không tìm thấy phiếu trả với mã PT{phieuTra.MaPhieuTra}.");
        
        // Cập nhật thông tin phiếu trả
        existing.NgayTra = phieuTra.NgayTra;
        existing.MaDocGia = phieuTra.MaDocGia;
        existing.MaNhanVien = phieuTra.MaNhanVien;
        existing.TienPhatKyNay = phieuTra.TienPhatKyNay;
        existing.TongNo = phieuTra.TongNo;
        
        // Remove old details and restore copy/book status
        var oldDetails = db.DbContext.DsChiTietPhieuTra.Where(ct => ct.MaPhieuTra == phieuTra.MaPhieuTra).ToList();
        foreach (var ct in oldDetails)
        {
            var banSao = await db.DbContext.DsBanSaoSach
                .Include(bs => bs.Sach)
                .FirstOrDefaultAsync(bs => bs.MaBanSao == ct.MaBanSao);
            if (banSao != null)
            {
                banSao.TinhTrang = "Đã mượn";
                db.DbContext.DsBanSaoSach.Update(banSao);
                var sach = await db.DbContext.DsSach.FindAsync(banSao.MaSach);
                if (sach != null)
                {
                    sach.SoLuongHienCo -= 1;
                    SE104_Library_Manager.Repositories.SachRepository.UpdateBookStatus(sach);
                    db.DbContext.DsSach.Update(sach);
                }
            }
        }
        db.DbContext.DsChiTietPhieuTra.RemoveRange(oldDetails);
        await db.DbContext.SaveChangesAsync();
        
        // Add new details and update copy/book status
        var newChiTietList = new List<ChiTietPhieuTra>();
        foreach (var ct in chiTietBanSao)
        {
            var newChiTiet = new ChiTietPhieuTra
            {
                MaPhieuTra = phieuTra.MaPhieuTra,
                MaPhieuMuon = ct.MaPhieuMuon,
                MaBanSao = ct.MaBanSao,
                TienPhat = ct.TienPhat
            };
            newChiTietList.Add(newChiTiet);
            
            var banSao = await db.DbContext.DsBanSaoSach
                .Include(bs => bs.Sach)
                .FirstOrDefaultAsync(bs => bs.MaBanSao == ct.MaBanSao);
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
        }
        
        await db.DbContext.DsChiTietPhieuTra.AddRangeAsync(newChiTietList);
        await db.DbContext.SaveChangesAsync();
        db.DbContext.ChangeTracker.Clear();
    }

    public async Task DeleteAsync(int maPhieuTra)
    {
        var phieuTra = await db.DbContext.DsPhieuTra.FindAsync(maPhieuTra);
        if (phieuTra != null)
        {
            phieuTra.DaXoa = true;
            await db.DbContext.SaveChangesAsync();
            db.DbContext.ChangeTracker.Clear();
        }
    }


}