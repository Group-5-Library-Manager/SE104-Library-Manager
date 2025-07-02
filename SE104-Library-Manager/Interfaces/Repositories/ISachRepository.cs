using SE104_Library_Manager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE104_Library_Manager.Interfaces.Repositories
{
    public interface ISachRepository
    {
        public Task<List<Sach>> GetAllAsync();
        public Task<Sach?> GetByIdAsync(int id);
        public Task AddAsync(Sach sach);
        public Task UpdateAsync(Sach sach);
        public Task DeleteAsync(int id);
        public Task ValidateSach(Sach sach);
    }
}
