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
    public class TacGiaRepository(DatabaseService dbService, IQuyDinhRepository quyDinhRepo) : ITacGiaRepository
    {
        public async Task<List<TacGia>> GetAllAsync()
        {
            return await dbService.DbContext.DsTacGia
                .AsNoTracking()
                .ToListAsync();
        }
        public Task<TacGia?> GetByIdAsync(int id)
        {
            return dbService.DbContext.DsTacGia
                .AsNoTracking()
                .FirstOrDefaultAsync(ldg => ldg.MaTacGia == id);
        }
        public async Task AddAsync(TacGia tacGia)
        {
            QuyDinh quyDinh = await quyDinhRepo.GetQuyDinhAsync();
            int count = await dbService.DbContext.DsTacGia.CountAsync();

            if (count >= quyDinh.SoTacGiaToiDa)
            {
                throw new InvalidOperationException($"Số lượng tác giả đã đạt giới hạn tối đa là {quyDinh.SoTacGiaToiDa}.");
            }

            if (tacGia == null) throw new ArgumentNullException("Tác giả không được là null");
            if (string.IsNullOrWhiteSpace(tacGia.TenTacGia))
            {
                throw new ArgumentException("Tên tác giả không được để trống.");
            }

            await dbService.DbContext.DsTacGia.AddAsync(tacGia);
            await dbService.DbContext.SaveChangesAsync();
            dbService.DbContext.ChangeTracker.Clear();
        }

        public async Task DeleteAsync(int id)
        {
            var existingTacGia = await dbService.DbContext.DsTacGia.FindAsync(id);

            if (existingTacGia == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy tác giả với mã TG{id}.");
            }

            if (await dbService.DbContext.DsSach.AnyAsync(dg => dg.MaTacGia == id))
            {
                throw new InvalidOperationException($"Không thể xóa tác giả với mã TG{id} vì có sách đang sử dụng tác giả này.");
            }

            dbService.DbContext.DsTacGia.Remove(existingTacGia);
            await dbService.DbContext.SaveChangesAsync();
            dbService.DbContext.ChangeTracker.Clear();
        }

        public async Task UpdateAsync(TacGia tacGia)
        {
            if (tacGia == null) throw new ArgumentNullException("Tác giả không được là null");
            if (string.IsNullOrWhiteSpace(tacGia.TenTacGia))
            {
                throw new ArgumentException("Tên tác giả không được để trống.");
            }

            var existingTacGia = await dbService.DbContext.DsTacGia.FindAsync(tacGia.MaTacGia);

            if (existingTacGia == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy tác giả với mã TG{tacGia.MaTacGia}.");
            }

            existingTacGia.TenTacGia = tacGia.TenTacGia;

            dbService.DbContext.DsTacGia.Update(existingTacGia);
            await dbService.DbContext.SaveChangesAsync();
            dbService.DbContext.ChangeTracker.Clear();
        }
    }
}
