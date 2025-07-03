using SE104_Library_Manager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE104_Library_Manager.Interfaces.Repositories
{
    public interface ITheLoaiRepository
    {
        public Task<List<TheLoai>> GetAllAsync();
        public Task<TheLoai?> GetByIdAsync(int id);
        public Task AddAsync(TheLoai theLoai);
        public Task UpdateAsync(TheLoai theLoai);
        public Task DeleteAsync(int id);
    }
}
