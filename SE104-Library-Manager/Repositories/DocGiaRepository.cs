using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;
using System.Text.RegularExpressions;

namespace SE104_Library_Manager.Repositories;

public class DocGiaRepository(DatabaseService dbService, IQuyDinhRepository quyDinhRepo) : IDocGiaRepository
{
    public async Task AddAsync(DocGia docGia)
    {
        await ValidateDocGia(docGia);

        if (await ExistsByEmailAsync(docGia.Email))
        {
            throw new InvalidOperationException($"Độc giả với email {docGia.Email} đã tồn tại.");
        }

        await dbService.DbContext.DsDocGia.AddAsync(docGia);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task DeleteAsync(int id)
    {
        var docGia = await dbService.DbContext.DsDocGia.FindAsync(id);

        if (docGia == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy độc giả với mã DG{id}.");
        }

        docGia.DaXoa = true;

        dbService.DbContext.Update(docGia);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await dbService.DbContext.DsDocGia.AnyAsync(dg => dg.Email.ToLower() == email.ToLower());
    }

    public async Task<List<DocGia>> GetAllAsync()
    {
        List<DocGia> dsDocGia = await dbService.DbContext.DsDocGia
            .AsNoTracking()
            .Include(dg => dg.LoaiDocGia)
            .Where(dg => !dg.DaXoa)
            .ToListAsync();

        return dsDocGia;
    }

    public async Task<DocGia?> GetByIdAsync(int id)
    {
        return await dbService.DbContext.DsDocGia
            .AsNoTracking()
            .Include(dg => dg.LoaiDocGia)
            .FirstOrDefaultAsync(dg => dg.MaDocGia == id && !dg.DaXoa);
    }

    public async Task UpdateAsync(DocGia docGia)
    {
        await ValidateDocGia(docGia);

        var existingDocGia = await dbService.DbContext.DsDocGia.FindAsync(docGia.MaDocGia);

        if (existingDocGia == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy độc giả với mã DG{docGia.MaDocGia}.");
        }

        if (await ExistsByEmailAsync(docGia.Email) && docGia.Email.ToLower() != existingDocGia.Email.ToLower())
        {
            throw new InvalidOperationException($"Độc giả với email {docGia.Email} đã tồn tại.");
        }

        existingDocGia.TenDocGia = docGia.TenDocGia;
        existingDocGia.DiaChi = docGia.DiaChi;
        existingDocGia.Email = docGia.Email;
        existingDocGia.MaLoaiDocGia = docGia.MaLoaiDocGia;
        existingDocGia.NgaySinh = docGia.NgaySinh;
        existingDocGia.NgayLapThe = docGia.NgayLapThe;
        existingDocGia.TongNo = docGia.TongNo;

        dbService.DbContext.DsDocGia.Update(existingDocGia);
        await dbService.DbContext.SaveChangesAsync();
        dbService.DbContext.ChangeTracker.Clear();
    }

    public async Task ValidateDocGia(DocGia docGia)
    {
        QuyDinh quyDinh = await quyDinhRepo.GetQuyDinhAsync();

        if (docGia == null) throw new ArgumentNullException("Độc giả không được là null");

        if (string.IsNullOrWhiteSpace(docGia.TenDocGia))
        {
            throw new ArgumentException("Tên độc giả không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(docGia.DiaChi))
        {
            throw new ArgumentException("Địa chỉ độc giả không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(docGia.Email))
        {
            throw new ArgumentException("Email độc giả không được để trống.");
        }   
        if (!Regex.IsMatch(docGia.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            throw new ArgumentException("Email độc giả không hợp lệ.");
        }

        if (docGia.MaLoaiDocGia <= 0)
        {
            throw new ArgumentException("Mã loại độc giả không hợp lệ.");
        }

        if (docGia.NgaySinh == default)
        {
            throw new ArgumentException("Ngày sinh độc giả không được để trống.");
        }
        if (docGia.NgaySinh > DateOnly.FromDateTime(DateTime.Now))
        {
            throw new ArgumentException("Ngày sinh độc giả không được lớn hơn ngày hiện tại.");
        }

        var minAge = quyDinh.TuoiDocGiaToiThieu;
        var maxAge = quyDinh.TuoiDocGiaToiDa;
        int currentAge = DateOnly.FromDateTime(DateTime.Now).Year - docGia.NgaySinh.Year;

        // If the birthday hasn't occurred yet this year, subtract one from the age
        if (DateOnly.FromDateTime(DateTime.Now) < docGia.NgaySinh.AddYears(currentAge))
        {
            currentAge--;
        }

        if (currentAge < minAge || currentAge > maxAge)
        {
            throw new ArgumentException($"Độc giả phải từ {minAge} đến {maxAge} tuổi.");
        }

        if (docGia.NgayLapThe == default)
        {
            throw new ArgumentException("Ngày lập thẻ độc giả không được để trống.");
        }
        if (docGia.NgayLapThe > DateOnly.FromDateTime(DateTime.Now))
        {
            throw new ArgumentException("Ngày lập thẻ độc giả không được lớn hơn ngày hiện tại.");
        }

        if (docGia.TongNo < 0)
        {
            throw new ArgumentException("Tổng nợ độc giả không được âm.");
        }
    }
}
