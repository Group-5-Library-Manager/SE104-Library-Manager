using SE104_Library_Manager.Entities;

namespace SE104_Library_Manager.Interfaces.Repositories;
public interface IChiTietPhieuTraRepository
{
    Task<List<ChiTietPhieuTra>> GetByPhieuTraAsync(int maPhieuTra);
    Task<List<ChiTietPhieuTra>> GetAllByDocGiaAsync(int maDocGia);
    Task AddAsync(ChiTietPhieuTra chiTietPhieuTra);
    Task AddRangeAsync(IEnumerable<ChiTietPhieuTra> dsChiTietPhieuTra);
    Task DeleteByPhieuTraAsync(int maPhieuTra);
    Task<bool> HasCopiesBeenBorrowedAgainAsync(int maPhieuTra);
}