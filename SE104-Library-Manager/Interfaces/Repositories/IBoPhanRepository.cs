using SE104_Library_Manager.Entities;

namespace SE104_Library_Manager.Interfaces.Repositories;

public interface IBoPhanRepository
{
    Task<List<BoPhan>> GetAllAsync();
    Task<BoPhan?> GetByIdAsync(int id);
    Task AddAsync(BoPhan boPhan);
    Task DeleteAsync(int id);
    Task UpdateAsync(BoPhan boPhan);
}
