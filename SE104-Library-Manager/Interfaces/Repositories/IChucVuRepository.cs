using SE104_Library_Manager.Entities;

namespace SE104_Library_Manager.Interfaces.Repositories;

public interface IChucVuRepository
{
    Task<List<ChucVu>> GetAllAsync();
    Task<ChucVu?> GetByIdAsync(int id);
    Task AddAsync(ChucVu chucVu);
    Task DeleteAsync(int id);
    Task UpdateAsync(ChucVu chucVu);
}
