using SE104_Library_Manager.Entities;

namespace SE104_Library_Manager.Interfaces.Repositories;

public interface IPhieuTraRepository
{
    Task<List<PhieuTra>> GetAllAsync();
    Task<PhieuTra?> GetByIdAsync(int maPhieuTra);
    Task<List<DocGia>> GetDocGiaDangCoSachMuonAsync();
    Task<List<ChiTietPhieuMuon>> GetSachDangMuonByDocGiaAsync(int maDocGia);
    Task<ChiTietPhieuMuon?> GetChiTietMuonMoiNhatChuaTraAsync(int maSach);
    Task AddAsync(PhieuTra phieuTra);
    Task UpdateAsync(PhieuTra phieuTra);
    Task DeleteAsync(int maPhieuTra);
}