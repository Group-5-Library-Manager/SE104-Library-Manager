using SE104_Library_Manager.Entities;

namespace SE104_Library_Manager.Interfaces.Repositories;

public interface ILoaiDocGiaRepository
{
    public Task<List<LoaiDocGia>> GetAllAsync();
    public Task<LoaiDocGia?> GetByIdAsync(int id);
    public Task AddAsync(LoaiDocGia loaiDocGia);
    public Task UpdateAsync(LoaiDocGia loaiDocGia);
    public Task DeleteAsync(int id);
}
