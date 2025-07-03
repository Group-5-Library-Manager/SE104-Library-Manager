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
    public class TheLoaiRepository(DatabaseService dbService, IQuyDinhRepository quyDinhRepo) : ITheLoaiRepository
    {
        public async Task<List<TheLoai>> GetAllAsync()
        {
            return await dbService.DbContext.DsTheLoai
                .AsNoTracking()
                .ToListAsync();
        }
        public Task<TheLoai?> GetByIdAsync(int id)
        {
            return dbService.DbContext.DsTheLoai
                .AsNoTracking()
                .FirstOrDefaultAsync(ldg => ldg.MaTheLoai == id);
        }
        public async Task AddAsync(TheLoai theLoai)
        {
            QuyDinh quyDinh = await quyDinhRepo.GetQuyDinhAsync();
            int count = await dbService.DbContext.DsTheLoai.CountAsync();

            if (count >= quyDinh.SoTheLoaiToiDa)
            {
                throw new InvalidOperationException($"Số lượng thể loại đã đạt giới hạn tối đa là {quyDinh.SoTheLoaiToiDa}.");
            }

            if (theLoai == null) throw new ArgumentNullException("Thể loại không được là null");
            if (string.IsNullOrWhiteSpace(theLoai.TenTheLoai))
            {
                throw new ArgumentException("Tên thể loại không được để trống.");
            }

            var exists = await dbService.DbContext.DsTheLoai.AnyAsync(ldg => ldg.TenTheLoai.ToLower() == theLoai.TenTheLoai.ToLower());

            if (exists)
            {
                throw new InvalidOperationException($"Thể loại với tên {theLoai.TenTheLoai} đã tồn tại.");
            }

            await dbService.DbContext.DsTheLoai.AddAsync(theLoai);
            await dbService.DbContext.SaveChangesAsync();
            dbService.DbContext.ChangeTracker.Clear();
        }

        public async Task DeleteAsync(int id)
        {
            var existingTheLoai = await dbService.DbContext.DsTheLoai.FindAsync(id);

            if (existingTheLoai == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy thể loại với mã TL{id}.");
            }

            if (await dbService.DbContext.DsSach.AnyAsync(dg => dg.MaTheLoai == id))
            {
                throw new InvalidOperationException($"Không thể xóa thể loại với mã TL{id} vì có sách đang sử dụng loại này.");
            }

            dbService.DbContext.DsTheLoai.Remove(existingTheLoai);
            await dbService.DbContext.SaveChangesAsync();
            dbService.DbContext.ChangeTracker.Clear();
        }

        public async Task UpdateAsync(TheLoai theLoai)
        {
            if (theLoai == null) throw new ArgumentNullException("Thể loại không được là null");
            if (string.IsNullOrWhiteSpace(theLoai.TenTheLoai))
            {
                throw new ArgumentException("Tên thể loại không được để trống.");
            }

            if (await dbService.DbContext.DsTheLoai.AnyAsync(ldg => ldg.TenTheLoai.ToLower() == theLoai.TenTheLoai.ToLower() && ldg.MaTheLoai != theLoai.MaTheLoai))
            {
                throw new InvalidOperationException($"Thể loại với tên {theLoai.TenTheLoai} đã tồn tại.");
            }

            var existingTheLoai = await dbService.DbContext.DsTheLoai.FindAsync(theLoai.MaTheLoai);

            if (existingTheLoai == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy thể loại với mã TL{theLoai.MaTheLoai}.");
            }

            existingTheLoai.TenTheLoai = theLoai.TenTheLoai;

            dbService.DbContext.DsTheLoai.Update(existingTheLoai);
            await dbService.DbContext.SaveChangesAsync();
            dbService.DbContext.ChangeTracker.Clear();
        }
    }
}
