using SE104_Library_Manager.Entities;

namespace SE104_Library_Manager.Interfaces.Repositories;

public interface IDocGiaRepository
{
    public Task<List<DocGia>> GetAllAsync();
    public Task<DocGia?> GetByIdAsync(int id);
    public Task<bool> ExistsByEmailAsync(string email);
    public Task AddAsync(DocGia docGia);
    public Task UpdateAsync(DocGia docGia);
    public Task DeleteAsync(int id);
    public Task ValidateDocGia(DocGia docGia);
}
