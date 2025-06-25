using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;

namespace SE104_Library_Manager.Repositories
{
    public class PhieuMuonRepository(DatabaseService dbService, IQuyDinhRepository quyDinhRepo) : IPhieuMuonRepository
    {
        public async Task AddAsync(PhieuMuon phieuMuon, List<ChiTietPhieuMuon> dsChiTietPhieuMuon)
        {
            await ValidatePhieuMuon(phieuMuon, dsChiTietPhieuMuon);

            using var transaction = await dbService.DbContext.Database.BeginTransactionAsync();
            try
            {
                await dbService.DbContext.DsPhieuMuon.AddAsync(phieuMuon);
                await dbService.DbContext.SaveChangesAsync();

                foreach (var chiTiet in dsChiTietPhieuMuon)
                {
                    chiTiet.MaPhieuMuon = phieuMuon.MaPhieuMuon;
                    await dbService.DbContext.DsChiTietPhieuMuon.AddAsync(chiTiet);
                    await this.UpdateBookStatusAsync(chiTiet.MaSach, "Đã mượn");
                }

                await dbService.DbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            finally
            {
                dbService.DbContext.ChangeTracker.Clear();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using var transaction = await dbService.DbContext.Database.BeginTransactionAsync();
            try
            {
                var phieuMuon = await dbService.DbContext.DsPhieuMuon
                    .Include(pm => pm.DsChiTietPhieuMuon)
                    .FirstOrDefaultAsync(pm => pm.MaPhieuMuon == id);

                if (phieuMuon == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy phiếu mượn với mã PM{id}.");
                }

                // Mark the borrow receipt as deleted
                phieuMuon.DaXoa = true;
                var chiTietPMs = phieuMuon.DsChiTietPhieuMuon;

                // Update all borrowed books back to available status
                foreach (var chiTiet in chiTietPMs)
                {
                    await this.UpdateBookStatusAsync(chiTiet.MaSach, "Có sẵn");
                }

                dbService.DbContext.DsChiTietPhieuMuon.RemoveRange(chiTietPMs);
                await dbService.DbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            finally
            {
                dbService.DbContext.ChangeTracker.Clear();
            }
        }

        public async Task<List<PhieuMuon>> GetAllAsync()
        {
            return await dbService.DbContext.DsPhieuMuon
            .AsNoTracking()
            .Include(pm => pm.DocGia)
            .Include(pm => pm.NhanVien)
            .Include(pm => pm.DsChiTietPhieuMuon)
                .ThenInclude(ct => ct.Sach)
            .Where(pm => !pm.DaXoa)
            .ToListAsync();
        }

        public async Task<PhieuMuon?> GetByIdAsync(int id)
        {
            return await dbService.DbContext.DsPhieuMuon
            .AsNoTracking()
            .Include(pm => pm.DocGia)
            .Include(pm => pm.NhanVien)
            .Include(pm => pm.DsChiTietPhieuMuon)
                .ThenInclude(ct => ct.Sach)
            .FirstOrDefaultAsync(pm => pm.MaPhieuMuon == id && !pm.DaXoa);
        }

        public async Task<List<PhieuMuon>> GetByReaderIdAsync(int maDocGia)
        {
            return await dbService.DbContext.DsPhieuMuon
            .AsNoTracking()
            .Include(pm => pm.DocGia)
            .Include(pm => pm.NhanVien)
            .Include(pm => pm.DsChiTietPhieuMuon)
                .ThenInclude(ct => ct.Sach)
            .Where(pm => pm.MaDocGia == maDocGia && !pm.DaXoa)
            .ToListAsync();
        }

        public async Task UpdateAsync(PhieuMuon phieuMuon)
        {
            var existingPhieuMuon = await dbService.DbContext.DsPhieuMuon.FindAsync(phieuMuon.MaPhieuMuon);

            if (existingPhieuMuon == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy phiếu mượn với mã PM{phieuMuon.MaPhieuMuon}.");
            }

            existingPhieuMuon.NgayMuon = phieuMuon.NgayMuon;
            existingPhieuMuon.MaDocGia = phieuMuon.MaDocGia;
            existingPhieuMuon.MaNhanVien = phieuMuon.MaNhanVien;

            dbService.DbContext.Update(existingPhieuMuon);
            await dbService.DbContext.SaveChangesAsync();
            dbService.DbContext.ChangeTracker.Clear();
        }

        // New overloaded UpdateAsync method that handles both PhieuMuon and its details
        public async Task UpdateAsync(PhieuMuon phieuMuon, List<ChiTietPhieuMuon> dsChiTietPhieuMuon)
        {
            await ValidatePhieuMuonForUpdate(phieuMuon, dsChiTietPhieuMuon);

            using var transaction = await dbService.DbContext.Database.BeginTransactionAsync();
            try
            {
                // Update the main PhieuMuon record
                var existingPhieuMuon = await dbService.DbContext.DsPhieuMuon.FindAsync(phieuMuon.MaPhieuMuon);
                if (existingPhieuMuon == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy phiếu mượn với mã PM{phieuMuon.MaPhieuMuon}.");
                }

                existingPhieuMuon.NgayMuon = phieuMuon.NgayMuon;
                existingPhieuMuon.MaDocGia = phieuMuon.MaDocGia;
                existingPhieuMuon.MaNhanVien = phieuMuon.MaNhanVien;

                // Get existing details to update book statuses
                var existingDetails = await dbService.DbContext.DsChiTietPhieuMuon
                    .Where(ct => ct.MaPhieuMuon == phieuMuon.MaPhieuMuon)
                    .ToListAsync();

                // Remove existing details
                dbService.DbContext.DsChiTietPhieuMuon.RemoveRange(existingDetails);

                // Update book statuses for previously borrowed books (make them available again)
                foreach (var existingDetail in existingDetails)
                {
                    await this.UpdateBookStatusAsync(existingDetail.MaSach, "Có sẵn");
                }

                // Add new details
                foreach (var chiTiet in dsChiTietPhieuMuon)
                {
                    chiTiet.MaPhieuMuon = phieuMuon.MaPhieuMuon;
                    await dbService.DbContext.DsChiTietPhieuMuon.AddAsync(chiTiet);
                    await this.UpdateBookStatusAsync(chiTiet.MaSach, "Đã mượn");
                }

                await dbService.DbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            finally
            {
                dbService.DbContext.ChangeTracker.Clear();
            }
        }

        public async Task ValidatePhieuMuon(PhieuMuon phieuMuon, List<ChiTietPhieuMuon> dsChiTietPhieuMuon)
        {
            QuyDinh quyDinh = await quyDinhRepo.GetQuyDinhAsync();

            if (phieuMuon == null) throw new ArgumentNullException("Phiếu mượn không được là null");

            if (dsChiTietPhieuMuon == null || !dsChiTietPhieuMuon.Any())
            {
                throw new ArgumentException("Phiếu mượn phải có ít nhất một sách");
            }

            if (dsChiTietPhieuMuon.Count > quyDinh.SoSachMuonToiDa)
            {
                throw new ArgumentException($"Số sách mượn không được vượt quá {quyDinh.SoSachMuonToiDa}");
            }

            // Kiểm tra độc giả có tồn tại không
            var docGia = await dbService.DbContext.DsDocGia.FindAsync(phieuMuon.MaDocGia);
            if (docGia == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy độc giả với mã DG{phieuMuon.MaDocGia}.");
            }

            // Kiểm tra thẻ độc giả có hết hạn không
            var ngayLapThe = docGia.NgayLapThe.ToDateTime(TimeOnly.MinValue);
            var ngayHetHan = ngayLapThe.AddMonths(quyDinh.ThoiHanTheDocGia);
            if (ngayHetHan < DateTime.Now.Date)
            {
                throw new InvalidOperationException($"Thẻ độc giả của {docGia.TenDocGia} đã hết hạn vào ngày {ngayHetHan:dd/MM/yyyy}. Vui lòng gia hạn thẻ trước khi mượn sách.");
            }

            // Kiểm tra độc giả có sách quá hạn không
            var hasSachQuaHan = await HasOverdueBooksAsync(phieuMuon.MaDocGia);
            if (hasSachQuaHan)
            {
                throw new InvalidOperationException($"Độc giả {docGia.TenDocGia} có sách quá hạn. Vui lòng trả sách quá hạn trước khi mượn sách mới.");
            }

            // Kiểm tra nhân viên có tồn tại không
            var nhanVien = await dbService.DbContext.DsNhanVien.FindAsync(phieuMuon.MaNhanVien);
            if (nhanVien == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy nhân viên với mã NV{phieuMuon.MaNhanVien}.");
            }

            // Kiểm tra sách có tồn tại và có sẵn không
            foreach (var chiTiet in dsChiTietPhieuMuon)
            {
                var sach = await dbService.DbContext.DsSach.FindAsync(chiTiet.MaSach);
                if (sach == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy sách với mã S{chiTiet.MaSach}.");
                }

                if (sach.TrangThai != "Có sẵn")
                {
                    throw new InvalidOperationException($"Sách {sach.TenSach} (mã S{sach.MaSach}) không có sẵn để mượn.");
                }
            }

            // Kiểm tra số sách đang mượn của độc giả
            var soSachDangMuon = await dbService.DbContext.DsPhieuMuon
                .Where(pm => pm.MaDocGia == phieuMuon.MaDocGia && !pm.DaXoa)
                .SelectMany(pm => pm.DsChiTietPhieuMuon)
                .CountAsync();

            if (soSachDangMuon + dsChiTietPhieuMuon.Count > quyDinh.SoSachMuonToiDa)
            {
                throw new InvalidOperationException($"Độc giả {docGia.TenDocGia} đã mượn {soSachDangMuon} sách. Không thể mượn thêm {dsChiTietPhieuMuon.Count} sách nữa (vượt quá {quyDinh.SoSachMuonToiDa}).");
            }
        }

        // Validation method specifically for updates
        private async Task ValidatePhieuMuonForUpdate(PhieuMuon phieuMuon, List<ChiTietPhieuMuon> dsChiTietPhieuMuon)
        {
            QuyDinh quyDinh = await quyDinhRepo.GetQuyDinhAsync();

            if (phieuMuon == null) throw new ArgumentNullException("Phiếu mượn không được là null");

            if (dsChiTietPhieuMuon == null || !dsChiTietPhieuMuon.Any())
            {
                throw new ArgumentException("Phiếu mượn phải có ít nhất một sách");
            }

            if (dsChiTietPhieuMuon.Count > quyDinh.SoSachMuonToiDa)
            {
                throw new ArgumentException($"Số sách mượn không được vượt quá {quyDinh.SoSachMuonToiDa}");
            }

            // Kiểm tra độc giả có tồn tại không
            var docGia = await dbService.DbContext.DsDocGia.FindAsync(phieuMuon.MaDocGia);
            if (docGia == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy độc giả với mã DG{phieuMuon.MaDocGia}.");
            }

            // Kiểm tra thẻ độc giả có hết hạn không
            var ngayLapThe = docGia.NgayLapThe.ToDateTime(TimeOnly.MinValue);
            var ngayHetHan = ngayLapThe.AddMonths(quyDinh.ThoiHanTheDocGia);
            if (ngayHetHan < DateTime.Now.Date)
            {
                throw new InvalidOperationException($"Thẻ độc giả của {docGia.TenDocGia} đã hết hạn vào ngày {ngayHetHan:dd/MM/yyyy}. Vui lòng gia hạn thẻ trước khi cập nhật phiếu mượn.");
            }

            // Kiểm tra độc giả có sách quá hạn không (exclude current borrow record)
            var hasSachQuaHan = await HasOverdueBooksAsync(phieuMuon.MaDocGia, phieuMuon.MaPhieuMuon);
            if (hasSachQuaHan)
            {
                throw new InvalidOperationException($"Độc giả {docGia.TenDocGia} có sách quá hạn. Vui lòng trả sách quá hạn trước khi cập nhật phiếu mượn.");
            }

            // Kiểm tra nhân viên có tồn tại không
            var nhanVien = await dbService.DbContext.DsNhanVien.FindAsync(phieuMuon.MaNhanVien);
            if (nhanVien == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy nhân viên với mã NV{phieuMuon.MaNhanVien}.");
            }

            // Get existing borrowed books for this borrow record
            var existingBorrowedBookIds = await dbService.DbContext.DsChiTietPhieuMuon
                .Where(ct => ct.MaPhieuMuon == phieuMuon.MaPhieuMuon)
                .Select(ct => ct.MaSach)
                .ToListAsync();

            // Kiểm tra sách có tồn tại và có sẵn không (cho phép sách đang được mượn trong cùng phiếu mượn này)
            foreach (var chiTiet in dsChiTietPhieuMuon)
            {
                var sach = await dbService.DbContext.DsSach.FindAsync(chiTiet.MaSach);
                if (sach == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy sách với mã S{chiTiet.MaSach}.");
                }

                // Allow books that are currently borrowed by this same borrow record, or books that are available
                if (sach.TrangThai != "Có sẵn" && !existingBorrowedBookIds.Contains(chiTiet.MaSach))
                {
                    throw new InvalidOperationException($"Sách {sach.TenSach} (mã S{sach.MaSach}) không có sẵn để mượn.");
                }
            }

            // Kiểm tra số sách đang mượn của độc giả (excluding current borrow record)
            var soSachDangMuon = await dbService.DbContext.DsPhieuMuon
                .Where(pm => pm.MaDocGia == phieuMuon.MaDocGia && !pm.DaXoa && pm.MaPhieuMuon != phieuMuon.MaPhieuMuon)
                .SelectMany(pm => pm.DsChiTietPhieuMuon)
                .CountAsync();

            if (soSachDangMuon + dsChiTietPhieuMuon.Count > quyDinh.SoSachMuonToiDa)
            {
                throw new InvalidOperationException($"Độc giả {docGia.TenDocGia} đã mượn {soSachDangMuon} sách. Không thể mượn thêm {dsChiTietPhieuMuon.Count} sách nữa (vượt quá {quyDinh.SoSachMuonToiDa}).");
            }
        }

        // Kiểm tra độc giả có sách quá hạn không
        public async Task<bool> HasOverdueBooksAsync(int maDocGia, int? excludePhieuMuonId = null)
        {
            var quyDinh = await quyDinhRepo.GetQuyDinhAsync();
            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            var query = dbService.DbContext.DsPhieuMuon
                .Where(pm => pm.MaDocGia == maDocGia && !pm.DaXoa);

            // Exclude specific phieu muon if provided (for update scenarios)
            if (excludePhieuMuonId.HasValue)
            {
                query = query.Where(pm => pm.MaPhieuMuon != excludePhieuMuonId.Value);
            }

            var hasOverdueBooks = await query
                .Where(pm => pm.NgayMuon.AddDays(quyDinh.SoNgayMuonToiDa) < currentDate)
                .SelectMany(pm => pm.DsChiTietPhieuMuon)
                .AnyAsync();

            return hasOverdueBooks;
        }

        // Lấy danh sách sách quá hạn của độc giả
        public async Task<List<PhieuMuon>> GetOverdueBooksAsync(int maDocGia)
        {
            var quyDinh = await quyDinhRepo.GetQuyDinhAsync();
            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            return await dbService.DbContext.DsPhieuMuon
                .AsNoTracking()
                .Include(pm => pm.DocGia)
                .Include(pm => pm.NhanVien)
                .Include(pm => pm.DsChiTietPhieuMuon)
                    .ThenInclude(ct => ct.Sach)
                .Where(pm => pm.MaDocGia == maDocGia && !pm.DaXoa &&
                           pm.NgayMuon.AddDays(quyDinh.SoNgayMuonToiDa) < currentDate)
                .ToListAsync();
        }

        // Lấy tất cả sách quá hạn trong hệ thống
        public async Task<List<PhieuMuon>> GetAllOverdueBooksAsync()
        {
            var quyDinh = await quyDinhRepo.GetQuyDinhAsync();
            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            return await dbService.DbContext.DsPhieuMuon
                .AsNoTracking()
                .Include(pm => pm.DocGia)
                .Include(pm => pm.NhanVien)
                .Include(pm => pm.DsChiTietPhieuMuon)
                    .ThenInclude(ct => ct.Sach)
                .Where(pm => !pm.DaXoa && pm.NgayMuon.AddDays(quyDinh.SoNgayMuonToiDa) < currentDate)
                .ToListAsync();
        }

        #region temp for sach repo
        public async Task<List<Sach>> GetAllBooksAsync()
        {
            return await dbService.DbContext.DsSach
            .AsNoTracking()
            .Include(s => s.TheLoai)
            .Include(s => s.TacGia)
            .Include(s => s.NhaXuatBan)
            .Where(s => !s.DaXoa)
            .ToListAsync();
        }

        public async Task<Sach?> GetBookByIdAsync(int id)
        {
            return await dbService.DbContext.DsSach
            .AsNoTracking()
            .Include(s => s.TheLoai)
            .Include(s => s.TacGia)
            .Include(s => s.NhaXuatBan)
            .FirstOrDefaultAsync(s => s.MaSach == id && !s.DaXoa);
        }

        public async Task<List<Sach>> GetAvailableBooksAsync()
        {
            return await dbService.DbContext.DsSach
            .AsNoTracking()
            .Include(s => s.TheLoai)
            .Include(s => s.TacGia)
            .Include(s => s.NhaXuatBan)
            .Where(s => !s.DaXoa && s.TrangThai == "Có sẵn")
            .ToListAsync();
        }

        public async Task<bool> IsBookAvailableAsync(int maSach)
        {
            var sach = await dbService.DbContext.DsSach.FindAsync(maSach);
            return sach != null && !sach.DaXoa && sach.TrangThai == "Có sẵn";
        }

        public async Task UpdateBookStatusAsync(int maSach, string trangThai)
        {
            var sach = await dbService.DbContext.DsSach.FindAsync(maSach);

            if (sach == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy sách với mã S{maSach}.");
            }

            sach.TrangThai = trangThai;

            dbService.DbContext.Update(sach);
            await dbService.DbContext.SaveChangesAsync();
            dbService.DbContext.ChangeTracker.Clear();
        }
        #endregion
    }
}