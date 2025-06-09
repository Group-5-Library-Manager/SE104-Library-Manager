using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;

namespace SE104_Library_Manager.Repositories;

public class QuyDinhRepository(DatabaseService dbService) : IQuyDinhRepository
{
    public async Task<QuyDinh> GetQuyDinhAsync()
    {
        return await dbService.DbContext.DsQuyDinh
            .AsNoTracking()
            .FirstAsync(q => q.MaQuyDinh == 1);
    }
}
