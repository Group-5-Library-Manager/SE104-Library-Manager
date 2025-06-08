using SE104_Library_Manager.Entities;

namespace SE104_Library_Manager.Interfaces.Repositories;

public interface IVaiTroRepository
{
    public Task<List<VaiTro>> GetAllAsync();
}
