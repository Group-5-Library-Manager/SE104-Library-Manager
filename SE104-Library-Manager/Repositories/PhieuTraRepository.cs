using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;

namespace SE104_Library_Manager.Repositories;

public class PhieuTraRepository(DatabaseService db) : IPhieuTraRepository
{
    public async Task<List<PhieuTra>> GetAllAsync()
    {
        return await db.DbContext.DsPhieuTra
            .AsNoTracking()
            .Include(p => p.DsChiTietPhieuTra)
            .Include(p => p.DocGia)
            .Include(p => p.NhanVien)
            .Where(p => !p.DaXoa)
            .ToListAsync();
    }

    public async Task<PhieuTra?> GetByIdAsync(int maPhieuTra)
    {
        return await db.DbContext.DsPhieuTra
            .Include(p => p.DocGia)
            .Include (p => p.NhanVien)
            .Include(p => p.DsChiTietPhieuTra)
                .ThenInclude(ct => ct.Sach)
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
                    tr.MaPhieuMuon == ct.MaPhieuMuon && tr.MaSach == ct.MaSach))
            .Select(ct => ct.PhieuMuon.DocGia) 
            .Distinct()
            .ToListAsync();

        return docGias;
    }


    public async Task<List<ChiTietPhieuMuon>> GetSachDangMuonByDocGiaAsync(int maDocGia)
    {
        var docGia = await db.DbContext.DsDocGia.FindAsync(maDocGia);
        if (docGia == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy độc giả với mã DG{maDocGia}.");
        }

        var chiTietDangMuon = await db.DbContext.DsChiTietPhieuMuon
            .Include(ct => ct.Sach) 
            .Include(ct => ct.PhieuMuon)
            .Where(ct => ct.PhieuMuon.MaDocGia == maDocGia && !ct.PhieuMuon.DaXoa && !ct.Sach.DaXoa)
            .Where(ct => !db.DbContext.DsChiTietPhieuTra
                .Any(tr => tr.MaPhieuMuon == ct.MaPhieuMuon && tr.MaSach == ct.MaSach))
            .ToListAsync();

        return chiTietDangMuon;
    }


    public async Task<ChiTietPhieuMuon?> GetChiTietMuonMoiNhatChuaTraAsync(int maSach)
    {
        return await db.DbContext.DsChiTietPhieuMuon
            .Include(ct => ct.PhieuMuon)
            .Where(ct =>
                ct.MaSach == maSach &&
                !ct.PhieuMuon.DaXoa &&
                !db.DbContext.DsChiTietPhieuTra
                    .Any(tr => tr.MaPhieuMuon == ct.MaPhieuMuon && tr.MaSach == maSach))
            .OrderByDescending(ct => ct.PhieuMuon.NgayMuon)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(PhieuTra phieuTra)
    {
        await db.DbContext.DsPhieuTra.AddAsync(phieuTra);
        await db.DbContext.SaveChangesAsync();
        db.DbContext.ChangeTracker.Clear();
    }

    public async Task UpdateAsync(PhieuTra phieuTra)
    {
        var existing = await db.DbContext.DsPhieuTra.FindAsync(phieuTra.MaPhieuTra);
        if (existing == null)
        {
            throw new Exception($"Không tìm thấy phiếu trả với mã PT{phieuTra.MaPhieuTra}.");
        }

        existing.NgayTra = phieuTra.NgayTra;
        existing.MaDocGia = phieuTra.MaDocGia;
        existing.MaNhanVien = phieuTra.MaNhanVien;
        existing.TienPhatKyNay = phieuTra.TienPhatKyNay;

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