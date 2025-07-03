using SE104_Library_Manager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE104_Library_Manager.Interfaces.Repositories
{
    public interface INhaXuatBanRepository
    {
        public Task<List<NhaXuatBan>> GetAllAsync();
        public Task<NhaXuatBan?> GetByIdAsync(int id);
        public Task AddAsync(NhaXuatBan nhaXuatBan);
        public Task UpdateAsync(NhaXuatBan nhaXuatBan);
        public Task DeleteAsync(int id);
    }
}
