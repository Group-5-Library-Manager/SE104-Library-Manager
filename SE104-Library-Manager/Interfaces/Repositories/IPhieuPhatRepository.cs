using SE104_Library_Manager.Entities;

namespace SE104_Library_Manager.Interfaces.Repositories;

public interface IPhieuPhatRepository
{
    Task<List<PhieuPhat>> GetAllAsync();
    Task<PhieuPhat?> GetByIdAsync(int maPhieuPhat);
    Task<List<DocGia>> GetReadersWithDebtAsync();
    Task AddAsync(PhieuPhat phieuPhat);
    Task<bool> ExportAsync(PhieuPhat phieuPhat);
    Task DeleteAsync(int maPhieuPhat);
}