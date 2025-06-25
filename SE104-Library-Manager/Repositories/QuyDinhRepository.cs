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

    public async Task UpdateAsync(QuyDinh quyDinh)
    {
        var existing = await dbService.DbContext.DsQuyDinh.FindAsync(quyDinh.MaQuyDinh);
        if (existing != null)
        {
            dbService.DbContext.Entry(existing).CurrentValues.SetValues(quyDinh);
            await dbService.DbContext.SaveChangesAsync();
        }
    }
}
