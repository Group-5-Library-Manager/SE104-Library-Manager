using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE104_Library_Manager.Repositories
{
    public class NhaXuatBanRepository(DatabaseService dbService, IQuyDinhRepository quyDinhRepo) : INhaXuatBanRepository
    {
        public async Task<List<NhaXuatBan>> GetAllAsync()
        {
            return await dbService.DbContext.DsNhaXuatBan
                .AsNoTracking()
                .ToListAsync();
        }
        public Task<NhaXuatBan?> GetByIdAsync(int id)
        {
            return dbService.DbContext.DsNhaXuatBan
                .AsNoTracking()
                .FirstOrDefaultAsync(ldg => ldg.MaNhaXuatBan == id);
        }
        public async Task AddAsync(NhaXuatBan nhaXuatBan)
        {
            QuyDinh quyDinh = await quyDinhRepo.GetQuyDinhAsync();

            if (nhaXuatBan == null) throw new ArgumentNullException("Nhà xuất bản không được là null");
            if (string.IsNullOrWhiteSpace(nhaXuatBan.TenNhaXuatBan))
            {
                throw new ArgumentException("Tên nhà xuất bản không được để trống.");
            }

            await dbService.DbContext.DsNhaXuatBan.AddAsync(nhaXuatBan);
            await dbService.DbContext.SaveChangesAsync();
            dbService.DbContext.ChangeTracker.Clear();
        }

        public async Task DeleteAsync(int id)
        {
            var existingNhaXuatBan = await dbService.DbContext.DsNhaXuatBan.FindAsync(id);

            if (existingNhaXuatBan == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy nhà xuất bản với mã NXB{id}.");
            }

            if (await dbService.DbContext.DsSach.AnyAsync(dg => dg.MaNhaXuatBan == id))
            {
                throw new InvalidOperationException($"Không thể xóa nhà xuất bản với mã NXB{id} vì có sách đang sử dụng nhà xuất bản này.");
            }

            dbService.DbContext.DsNhaXuatBan.Remove(existingNhaXuatBan);
            await dbService.DbContext.SaveChangesAsync();
            dbService.DbContext.ChangeTracker.Clear();
        }

        public async Task UpdateAsync(NhaXuatBan nhaXuatBan)
        {
            if (nhaXuatBan == null) throw new ArgumentNullException("Nhà xuất bản không được là null");
            if (string.IsNullOrWhiteSpace(nhaXuatBan.TenNhaXuatBan))
            {
                throw new ArgumentException("Tên nhà xuất bản không được để trống.");
            }

            var existingNhaXuatBan = await dbService.DbContext.DsNhaXuatBan.FindAsync(nhaXuatBan.MaNhaXuatBan);

            if (existingNhaXuatBan == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy nhà xuất bản với mã NXB{nhaXuatBan.MaNhaXuatBan}.");
            }

            existingNhaXuatBan.TenNhaXuatBan = nhaXuatBan.TenNhaXuatBan;

            dbService.DbContext.DsNhaXuatBan.Update(existingNhaXuatBan);
            await dbService.DbContext.SaveChangesAsync();
            dbService.DbContext.ChangeTracker.Clear();
        }
    }
}
