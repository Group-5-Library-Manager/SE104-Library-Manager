using SE104_Library_Manager.Entities;

namespace SE104_Library_Manager.Interfaces.Repositories
{
    public interface IPhieuMuonRepository
    {
        Task<List<PhieuMuon>> GetAllAsync();
        Task<PhieuMuon?> GetByIdAsync(int id);
        Task<List<PhieuMuon>> GetByReaderIdAsync(int maDocGia);
        Task AddAsync(PhieuMuon phieuMuon, List<ChiTietPhieuMuon> dsChiTietPhieuMuon);
        Task UpdateAsync(PhieuMuon phieuMuon);
        Task UpdateAsync(PhieuMuon phieuMuon, List<ChiTietPhieuMuon> dsChiTietPhieuMuon);
        Task DeleteAsync(int id);
        Task ValidatePhieuMuon(PhieuMuon phieuMuon, List<ChiTietPhieuMuon> dsChiTietPhieuMuon);
        Task<bool> HasOverdueBooksAsync(int maDocGia, int? excludePhieuMuonId = null);
        Task<List<PhieuMuon>> GetOverdueBooksAsync(int maDocGia);
        Task<List<PhieuMuon>> GetAllOverdueBooksAsync();
        Task<bool> HasReturnedBooksAsync(int maPhieuMuon);

        //temp for sach repo
        public Task<List<Sach>> GetAllBooksAsync();
        public Task<Sach?> GetBookByIdAsync(int id);
        public Task<List<Sach>> GetAvailableBooksAsync();
        public Task<bool> IsBookAvailableAsync(int maSach);
        public Task UpdateBookStatusAsync(int maSach, string trangThai);
    }
}