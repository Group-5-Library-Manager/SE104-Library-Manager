using SE104_Library_Manager.Entities;
using SE104_Library_Manager.ViewModels.Return;

namespace SE104_Library_Manager.Interfaces.Repositories;

public interface IPhieuTraRepository
{
    Task<List<PhieuTra>> GetAllAsync();
    Task<PhieuTra?> GetByIdAsync(int maPhieuTra);
    Task<List<DocGia>> GetDocGiaDangCoSachMuonAsync();
    Task<List<ChiTietPhieuMuon>> GetBanSaoDangMuonByDocGiaAsync(int maDocGia);
    Task<ChiTietPhieuMuon?> GetChiTietMuonMoiNhatChuaTraAsync(int maBanSao);
    Task AddAsync(PhieuTra phieuTra, List<ChiTietPhieuTraInfo> chiTietBanSao);
    Task UpdateAsync(PhieuTra phieuTra, List<ChiTietPhieuTraInfo> chiTietBanSao);
    Task DeleteAsync(int maPhieuTra);
}