using SE104_Library_Manager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE104_Library_Manager.Interfaces.Repositories
{
    public interface ITacGiaRepository
    {
        public Task<List<TacGia>> GetAllAsync();
        public Task<TacGia?> GetByIdAsync(int id);
        public Task AddAsync(TacGia tacGia);
        public Task UpdateAsync(TacGia tacGia);
        public Task DeleteAsync(int id);
    }
}
