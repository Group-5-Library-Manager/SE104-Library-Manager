using SE104_Library_Manager.Entities;

namespace SE104_Library_Manager.Interfaces.Repositories;

public interface IBangCapRepository
{
    Task<List<BangCap>> GetAllAsync();
    Task<BangCap?> GetByIdAsync(int id);
    Task AddAsync(BangCap bangCap);
    Task DeleteAsync(int id);
    Task UpdateAsync(BangCap bangCap);
}
