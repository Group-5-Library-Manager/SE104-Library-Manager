using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SE104_Library_Manager.Repositories
{
    public class SachRepository(DatabaseService dbService, IQuyDinhRepository quyDinhRepo) : ISachRepository
    {
        public async Task<List<Sach>> GetAllAsync()
        {
            List<Sach> dsDocGia = await dbService.DbContext.DsSach
                .AsNoTracking()
                .Include(dg => dg.TheLoai)
                .Include(dg => dg.TacGia)
                .Include(dg => dg.NhaXuatBan)
                .Where(dg => !dg.DaXoa)
                .ToListAsync();

            return dsDocGia;
        }
        public async Task<Sach?> GetByIdAsync(int id)
        {
            return await dbService.DbContext.DsSach
                .AsNoTracking()
                .Include(dg => dg.TheLoai)
                .Include(dg => dg.TacGia)
                .Include(dg => dg.NhaXuatBan)
                .FirstOrDefaultAsync(dg => dg.MaSach == id && !dg.DaXoa);
        }
        public async Task AddAsync(Sach sach)
        {
            await ValidateSach(sach);

            await dbService.DbContext.DsSach.AddAsync(sach);
            await dbService.DbContext.SaveChangesAsync();
            dbService.DbContext.ChangeTracker.Clear();
        }
        public async Task DeleteAsync(int id)
        {
            var sach = await dbService.DbContext.DsSach.FindAsync(id);

            if (sach == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy sách với mã S{id}.");
            }

            sach.DaXoa = true;

            dbService.DbContext.Update(sach);
            await dbService.DbContext.SaveChangesAsync();
            dbService.DbContext.ChangeTracker.Clear();
        }
        public async Task UpdateAsync(Sach sach)
        {
            await ValidateSach(sach);

            var existingSach = await dbService.DbContext.DsSach.FindAsync(sach.MaSach);

            if (existingSach == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy sách với mã S{sach.MaSach}.");
            }

            existingSach.TenSach = sach.TenSach;
            existingSach.MaTheLoai = sach.MaTheLoai;
            existingSach.MaTacGia = sach.MaTacGia;
            existingSach.MaNhaXuatBan = sach.MaNhaXuatBan;
            existingSach.NamXuatBan = sach.NamXuatBan;
            existingSach.NgayNhap = sach.NgayNhap;
            existingSach.TriGia = sach.TriGia;
            existingSach.TrangThai = sach.TrangThai;

            dbService.DbContext.DsSach.Update(existingSach);
            await dbService.DbContext.SaveChangesAsync();
            dbService.DbContext.ChangeTracker.Clear();
        }
        public async Task ValidateSach(Sach sach)
        {
            QuyDinh quyDinh = await quyDinhRepo.GetQuyDinhAsync();

            if (sach == null) throw new ArgumentNullException("Sách không được là null");

            if (string.IsNullOrWhiteSpace(sach.TenSach))
            {
                throw new ArgumentException("Tên sách không được để trống.");
            }

            if (sach.MaTheLoai <= 0)
            {
                throw new ArgumentException("Mã thể loại không hợp lệ.");
            }

            if (sach.MaTacGia <= 0)
            {
                throw new ArgumentException("Mã tác giả không hợp lệ.");
            }

            if (sach.MaNhaXuatBan <= 0)
            {
                throw new ArgumentException("Mã nhà xuất bản không hợp lệ.");
            }

            if(sach.NamXuatBan < 0)
            {
                throw new ArgumentException("Năm xuất bản không hợp lệ.");
            }

            if (sach.NgayNhap == default)
            {
                throw new ArgumentException("Ngày nhập sách không được để trống.");
            }

            if(sach.TriGia < 0)
            {
                throw new ArgumentException("Trị giá không hợp lệ");
            }

            if (string.IsNullOrWhiteSpace(sach.TrangThai))
            {
                throw new ArgumentException("Trạng thái không được để trống.");
            }

            var minYearsPublished = quyDinh.SoNamXuatBanToiDa;
            int currentYearsPublished = DateOnly.FromDateTime(DateTime.Now).Year - sach.NamXuatBan;

            if(currentYearsPublished > minYearsPublished)
            {
                throw new ArgumentException($"Chỉ nhận các sách xuất bản trong vòng {minYearsPublished} năm.");
            }
        }
    }
}
