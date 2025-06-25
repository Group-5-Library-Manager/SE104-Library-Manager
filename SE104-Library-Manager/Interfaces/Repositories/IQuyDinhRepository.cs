using SE104_Library_Manager.Entities;

namespace SE104_Library_Manager.Interfaces.Repositories;


// !IMPORTANT: Do not need CREATE operations for this repository as QuyDinh is a singleton entity in the database.
public interface IQuyDinhRepository
{
    public Task<QuyDinh> GetQuyDinhAsync();
    Task UpdateAsync(QuyDinh quyDinh);
}
