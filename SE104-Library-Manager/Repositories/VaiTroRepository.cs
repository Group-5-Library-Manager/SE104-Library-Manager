using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;

namespace SE104_Library_Manager.Repositories;

public class VaiTroRepository(DatabaseService dbService) : IVaiTroRepository
{
    public async Task<List<VaiTro>> GetAllAsync()
    {
        return await dbService.DbContext.DsVaiTro
            .AsNoTracking()
            .ToListAsync();
    }
}
