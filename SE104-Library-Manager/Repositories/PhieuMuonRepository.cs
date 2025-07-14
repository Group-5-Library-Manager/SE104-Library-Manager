using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;

namespace SE104_Library_Manager.Repositories
{
    public class PhieuMuonRepository(DatabaseService dbService, IQuyDinhRepository quyDinhRepo) : IPhieuMuonRepository
    {
        public async Task AddAsync(PhieuMuon phieuMuon, List<BanSaoSach> selectedCopies)
        {
            await ValidatePhieuMuon(phieuMuon, selectedCopies);

            using var transaction = await dbService.DbContext.Database.BeginTransactionAsync();
            try
            {
                await dbService.DbContext.DsPhieuMuon.AddAsync(phieuMuon);
                await dbService.DbContext.SaveChangesAsync();

                foreach (var copy in selectedCopies)
                {
                    var chiTiet = new ChiTietPhieuMuon
                    {
                        MaPhieuMuon = phieuMuon.MaPhieuMuon,
                        MaBanSao = copy.MaBanSao
                    };
                    await dbService.DbContext.DsChiTietPhieuMuon.AddAsync(chiTiet);
                    // Set copy status
                    var banSao = await dbService.DbContext.DsBanSaoSach
                        .Include(bs => bs.Sach)
                        .FirstOrDefaultAsync(bs => bs.MaBanSao == copy.MaBanSao);
                    if (banSao != null)
                    {
                        banSao.TinhTrang = "Đã mượn";
                        dbService.DbContext.DsBanSaoSach.Update(banSao);
                        // Update book quantity
                        var sach = await dbService.DbContext.DsSach.FindAsync(banSao.MaSach);
                        if (sach != null)
                        {
                            sach.SoLuongHienCo -= 1;
                            SachRepository.UpdateBookStatus(sach);
                            dbService.DbContext.DsSach.Update(sach);
                        }
                    }
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
            // Không cho xóa nếu có bản sao đã trả
            var hasReturned = await HasReturnedBooksAsync(id);
            if (hasReturned)
                throw new InvalidOperationException($"Không thể xóa phiếu mượn PM{id} vì có bản sao đã được trả.");
            using var transaction = await dbService.DbContext.Database.BeginTransactionAsync();
            try
            {
                var phieuMuon = await dbService.DbContext.DsPhieuMuon
                    .Include(pm => pm.DsChiTietPhieuMuon)
                    .FirstOrDefaultAsync(pm => pm.MaPhieuMuon == id);
                if (phieuMuon == null)
                    throw new KeyNotFoundException($"Không tìm thấy phiếu mượn với mã PM{id}.");
                // Trả lại trạng thái cho các bản sao và sách
                foreach (var chiTiet in phieuMuon.DsChiTietPhieuMuon)
                {
                    var banSao = await dbService.DbContext.DsBanSaoSach
                        .Include(bs => bs.Sach)
                        .FirstOrDefaultAsync(bs => bs.MaBanSao == chiTiet.MaBanSao);
                    if (banSao != null)
                    {
                        banSao.TinhTrang = "Có sẵn";
                        dbService.DbContext.DsBanSaoSach.Update(banSao);
                        var sach = await dbService.DbContext.DsSach.FindAsync(banSao.MaSach);
                        if (sach != null)
                        {
                            sach.SoLuongHienCo += 1;
                            SachRepository.UpdateBookStatus(sach);
                            dbService.DbContext.DsSach.Update(sach);
                        }
                    }
                }
                phieuMuon.DaXoa = true;
                dbService.DbContext.Update(phieuMuon);
                dbService.DbContext.DsChiTietPhieuMuon.RemoveRange(phieuMuon.DsChiTietPhieuMuon);
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
                    .ThenInclude(ct => ct.BanSaoSach)
                        .ThenInclude(bs => bs.Sach)
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
                    .ThenInclude(ct => ct.BanSaoSach)
                        .ThenInclude(bs => bs.Sach)
                .FirstOrDefaultAsync(pm => pm.MaPhieuMuon == id && !pm.DaXoa);
        }

        public async Task<List<PhieuMuon>> GetByReaderIdAsync(int maDocGia)
        {
            return await dbService.DbContext.DsPhieuMuon
                .AsNoTracking()
                .Include(pm => pm.DocGia)
                .Include(pm => pm.NhanVien)
                .Include(pm => pm.DsChiTietPhieuMuon)
                    .ThenInclude(ct => ct.BanSaoSach)
                        .ThenInclude(bs => bs.Sach)
                .Where(pm => pm.MaDocGia == maDocGia && !pm.DaXoa)
                .ToListAsync();
        }

        public async Task UpdateAsync(PhieuMuon phieuMuon, List<BanSaoSach> selectedCopies)
        {
            // Check if any copy in this borrow has been returned
            var returnedBanSao = await dbService.DbContext.DsChiTietPhieuTra
                .Where(tr => tr.MaPhieuMuon == phieuMuon.MaPhieuMuon)
                .Select(tr => tr.MaBanSao)
                .ToListAsync();
            if (returnedBanSao.Any())
            {
                throw new InvalidOperationException("Không thể cập nhật phiếu mượn đã có bản sao được trả.");
            }

            await ValidatePhieuMuon(phieuMuon, selectedCopies);

            using var transaction = await dbService.DbContext.Database.BeginTransactionAsync();
            try
            {
                // Update main borrow record
                var existingPhieuMuon = await dbService.DbContext.DsPhieuMuon.FindAsync(phieuMuon.MaPhieuMuon);
                if (existingPhieuMuon == null)
                    throw new KeyNotFoundException($"Không tìm thấy phiếu mượn với mã PM{phieuMuon.MaPhieuMuon}.");
                existingPhieuMuon.NgayMuon = phieuMuon.NgayMuon;
                existingPhieuMuon.MaDocGia = phieuMuon.MaDocGia;
                existingPhieuMuon.MaNhanVien = phieuMuon.MaNhanVien;
                dbService.DbContext.Update(existingPhieuMuon);

                // Remove all old borrow details (since none have been returned)
                var oldDetails = await dbService.DbContext.DsChiTietPhieuMuon
                    .Where(ct => ct.MaPhieuMuon == phieuMuon.MaPhieuMuon)
                    .ToListAsync();
                foreach (var old in oldDetails)
                {
                    // Restore book/copy status
                    var banSao = await dbService.DbContext.DsBanSaoSach
                        .Include(bs => bs.Sach)
                        .FirstOrDefaultAsync(bs => bs.MaBanSao == old.MaBanSao);
                    if (banSao != null)
                    {
                        banSao.TinhTrang = "Có sẵn";
                        dbService.DbContext.DsBanSaoSach.Update(banSao);
                        var sach = await dbService.DbContext.DsSach.FindAsync(banSao.MaSach);
                        if (sach != null)
                        {
                            sach.SoLuongHienCo += 1;
                            SachRepository.UpdateBookStatus(sach);
                            dbService.DbContext.DsSach.Update(sach);
                        }
                    }
                }
                dbService.DbContext.DsChiTietPhieuMuon.RemoveRange(oldDetails);

                // Add new borrow details
                foreach (var copy in selectedCopies)
                {
                    var chiTiet = new ChiTietPhieuMuon
                    {
                        MaPhieuMuon = phieuMuon.MaPhieuMuon,
                        MaBanSao = copy.MaBanSao
                    };
                    await dbService.DbContext.DsChiTietPhieuMuon.AddAsync(chiTiet);
                    // Set copy status
                    var banSao = await dbService.DbContext.DsBanSaoSach
                        .Include(bs => bs.Sach)
                        .FirstOrDefaultAsync(bs => bs.MaBanSao == copy.MaBanSao);
                    if (banSao != null)
                    {
                        banSao.TinhTrang = "Đã mượn";
                        dbService.DbContext.DsBanSaoSach.Update(banSao);
                        var sach = await dbService.DbContext.DsSach.FindAsync(banSao.MaSach);
                        if (sach != null)
                        {
                            sach.SoLuongHienCo -= 1;
                            SachRepository.UpdateBookStatus(sach);
                            dbService.DbContext.DsSach.Update(sach);
                        }
                    }
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

        public async Task ValidatePhieuMuon(PhieuMuon phieuMuon, List<BanSaoSach> selectedCopies)
        {
            QuyDinh quyDinh = await quyDinhRepo.GetQuyDinhAsync();

            if (phieuMuon == null) throw new ArgumentNullException("Phiếu mượn không được là null");

            if (selectedCopies == null || !selectedCopies.Any())
            {
                throw new ArgumentException("Phiếu mượn phải có ít nhất một bản sao sách");
            }

            int tongSoLuongMuon = selectedCopies.Count;
            if (tongSoLuongMuon > quyDinh.SoSachMuonToiDa)
            {
                throw new ArgumentException($"Tổng số lượng sách mượn không được vượt quá {quyDinh.SoSachMuonToiDa}");
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

            // Kiểm tra bản sao có tồn tại và có sẵn không (hoặc đang được mượn trong phiếu này)
            foreach (var copy in selectedCopies)
            {
                var banSao = await dbService.DbContext.DsBanSaoSach
                    .Include(bs => bs.Sach)
                    .FirstOrDefaultAsync(bs => bs.MaBanSao == copy.MaBanSao);
                if (banSao == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy bản sao sách với mã BS{copy.MaBanSao}.");
                }
                
                // Check if this copy is already borrowed in this specific borrow receipt
                var isAlreadyInThisBorrow = await dbService.DbContext.DsChiTietPhieuMuon
                    .AnyAsync(ct => ct.MaPhieuMuon == phieuMuon.MaPhieuMuon && ct.MaBanSao == copy.MaBanSao);
                
                if (banSao.TinhTrang != "Có sẵn" && !isAlreadyInThisBorrow)
                {
                    throw new InvalidOperationException($"Bản sao sách BS{banSao.MaBanSao} không có sẵn để mượn.");
                }
            }

            // Kiểm tra số sách đang mượn của độc giả (không tính sách đã trả và không tính sách trong phiếu hiện tại)
            var soSachDangMuon = await dbService.DbContext.DsPhieuMuon
                .Where(pm => pm.MaDocGia == phieuMuon.MaDocGia && !pm.DaXoa && pm.MaPhieuMuon != phieuMuon.MaPhieuMuon)
                .SelectMany(pm => pm.DsChiTietPhieuMuon)
                .Where(ct => !dbService.DbContext.DsChiTietPhieuTra
                    .Any(tr => tr.MaPhieuMuon == ct.MaPhieuMuon && tr.MaBanSao == ct.MaBanSao))
                .CountAsync();

            if (soSachDangMuon + tongSoLuongMuon > quyDinh.SoSachMuonToiDa)
            {
                throw new InvalidOperationException($"Độc giả {docGia.TenDocGia} đã mượn {soSachDangMuon} sách. Không thể mượn thêm {tongSoLuongMuon} sách nữa (vượt quá {quyDinh.SoSachMuonToiDa}).");
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

            // Get overdue phieu muon IDs first
            var overduePhieuMuonIds = await query
                .Where(pm => pm.NgayMuon.AddDays(quyDinh.SoNgayMuonToiDa) < currentDate)
                .Select(pm => pm.MaPhieuMuon)
                .ToListAsync();

            if (!overduePhieuMuonIds.Any())
            {
                return false;
            }

            // Get all borrowed books from overdue borrow receipts
            var allOverdueBooks = await dbService.DbContext.DsChiTietPhieuMuon
                .Where(ct => overduePhieuMuonIds.Contains(ct.MaPhieuMuon))
                .Select(ct => new { ct.MaPhieuMuon, ct.MaBanSao })
                .ToListAsync();

            // Get all returned books for these borrow receipts
            var returnedBooks = await dbService.DbContext.DsChiTietPhieuTra
                .Where(ct => overduePhieuMuonIds.Contains(ct.MaPhieuMuon))
                .Select(ct => new { ct.MaPhieuMuon, ct.MaBanSao })
                .ToListAsync();

            // Create a lookup for returned books
            var returnedBooksLookup = returnedBooks
                .GroupBy(rb => rb.MaPhieuMuon)
                .ToDictionary(g => g.Key, g => g.Select(rb => rb.MaBanSao).ToHashSet());

            // Check if there are any unreturned overdue books
            foreach (var book in allOverdueBooks)
            {
                if (!returnedBooksLookup.TryGetValue(book.MaPhieuMuon, out var returnedBookIds))
                {
                    // No returned books for this borrow receipt, so this book is overdue
                    return true;
                }
                
                if (!returnedBookIds.Contains(book.MaBanSao))
                {
                    // This book is not in the returned books list, so it's overdue
                    return true;
                }
            }

            // All overdue books have been returned
            return false;
        }

        // Lấy danh sách sách quá hạn của độc giả
        public async Task<List<PhieuMuon>> GetOverdueBooksAsync(int maDocGia)
        {
            var quyDinh = await quyDinhRepo.GetQuyDinhAsync();
            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            var overduePhieuMuon = await dbService.DbContext.DsPhieuMuon
                .AsNoTracking()
                .Include(pm => pm.DocGia)
                .Include(pm => pm.NhanVien)
                .Include(pm => pm.DsChiTietPhieuMuon)
                    .ThenInclude(ct => ct.BanSaoSach)
                .Where(pm => pm.MaDocGia == maDocGia && !pm.DaXoa &&
                           pm.NgayMuon.AddDays(quyDinh.SoNgayMuonToiDa) < currentDate)
                .ToListAsync();

            // Get all returned books for these phieu muon in a single query
            var phieuMuonIds = overduePhieuMuon.Select(pm => pm.MaPhieuMuon).ToList();
            var returnedBooks = await dbService.DbContext.DsChiTietPhieuTra
                .Where(ct => phieuMuonIds.Contains(ct.MaPhieuMuon))
                .Select(ct => new { ct.MaPhieuMuon, ct.MaBanSao })
                .ToListAsync();

            // Create a lookup for returned books
            var returnedBooksLookup = returnedBooks
                .GroupBy(rb => rb.MaPhieuMuon)
                .ToDictionary(g => g.Key, g => g.Select(rb => rb.MaBanSao).ToHashSet());

            // Filter out books that have already been returned
            foreach (var phieuMuon in overduePhieuMuon)
            {
                if (returnedBooksLookup.TryGetValue(phieuMuon.MaPhieuMuon, out var returnedBookIds))
                {
                    phieuMuon.DsChiTietPhieuMuon = phieuMuon.DsChiTietPhieuMuon
                        .Where(ct => !returnedBookIds.Contains(ct.MaBanSao))
                        .ToList();
                }
            }

            // Return only phieu muon that still have unreturned books
            return overduePhieuMuon.Where(pm => pm.DsChiTietPhieuMuon.Any()).ToList();
        }

        // Lấy tất cả sách quá hạn trong hệ thống
        public async Task<List<PhieuMuon>> GetAllOverdueBooksAsync()
        {
            var quyDinh = await quyDinhRepo.GetQuyDinhAsync();
            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            var overduePhieuMuon = await dbService.DbContext.DsPhieuMuon
                .AsNoTracking()
                .Include(pm => pm.DocGia)
                .Include(pm => pm.NhanVien)
                .Include(pm => pm.DsChiTietPhieuMuon)
                    .ThenInclude(ct => ct.BanSaoSach)
                .Where(pm => !pm.DaXoa && pm.NgayMuon.AddDays(quyDinh.SoNgayMuonToiDa) < currentDate)
                .ToListAsync();

            // Get all returned books for these phieu muon in a single query
            var phieuMuonIds = overduePhieuMuon.Select(pm => pm.MaPhieuMuon).ToList();
            var returnedBooks = await dbService.DbContext.DsChiTietPhieuTra
                .Where(ct => phieuMuonIds.Contains(ct.MaPhieuMuon))
                .Select(ct => new { ct.MaPhieuMuon, ct.MaBanSao })
                .ToListAsync();

            // Create a lookup for returned books
            var returnedBooksLookup = returnedBooks
                .GroupBy(rb => rb.MaPhieuMuon)
                .ToDictionary(g => g.Key, g => g.Select(rb => rb.MaBanSao).ToHashSet());

            // Filter out books that have already been returned
            foreach (var phieuMuon in overduePhieuMuon)
            {
                if (returnedBooksLookup.TryGetValue(phieuMuon.MaPhieuMuon, out var returnedBookIds))
                {
                    phieuMuon.DsChiTietPhieuMuon = phieuMuon.DsChiTietPhieuMuon
                        .Where(ct => !returnedBookIds.Contains(ct.MaBanSao))
                        .ToList();
                }
            }

            // Return only phieu muon that still have unreturned books
            return overduePhieuMuon.Where(pm => pm.DsChiTietPhieuMuon.Any()).ToList();
        }

        public async Task<int> GetCurrentBorrowedCountAsync(int maDocGia)
        {
            // Đếm số lượng bản sao sách mà độc giả này đã mượn nhưng chưa trả
            return await dbService.DbContext.DsChiTietPhieuMuon
                .Where(ct =>
                    dbService.DbContext.DsPhieuMuon
                        .Any(pm => pm.MaPhieuMuon == ct.MaPhieuMuon && pm.MaDocGia == maDocGia && !pm.DaXoa)
                    &&
                    !dbService.DbContext.DsChiTietPhieuTra
                        .Any(tr => tr.MaPhieuMuon == ct.MaPhieuMuon && tr.MaBanSao == ct.MaBanSao)
                )
                .CountAsync();
        }


        public async Task<bool> HasReturnedBooksAsync(int maPhieuMuon)
        {
            // Kiểm tra xem phiếu mượn có bản sao nào đã trả hay không
            return await dbService.DbContext.DsChiTietPhieuTra
                .AnyAsync(ct => ct.MaPhieuMuon == maPhieuMuon);
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
            // Get books that have at least one available copy
            var availableBookIds = await dbService.DbContext.DsBanSaoSach
                .Where(bs => bs.TinhTrang == "Có sẵn")
                .Select(bs => bs.MaSach)
                .Distinct()
                .ToListAsync();

            return await dbService.DbContext.DsSach
                .AsNoTracking()
                .Include(s => s.TheLoai)
                .Include(s => s.TacGia)
                .Include(s => s.NhaXuatBan)
                .Where(s => !s.DaXoa && availableBookIds.Contains(s.MaSach))
                .ToListAsync();
        }

        public async Task<bool> IsBookAvailableAsync(int maSach)
        {
            var sach = await dbService.DbContext.DsSach.FindAsync(maSach);
            if (sach == null || sach.DaXoa)
                return false;

            // Check if there are any available copies
            var availableCopies = await dbService.DbContext.DsBanSaoSach
                .AnyAsync(bs => bs.MaSach == maSach && bs.TinhTrang == "Có sẵn");
            
            return availableCopies;
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

        public List<int> GetLockedBanSaoSachIds(int excludePhieuMuonId)
        {
            return dbService.DbContext.DsChiTietPhieuMuon
                .Where(ct => !ct.PhieuMuon.DaXoa &&
                             !dbService.DbContext.DsChiTietPhieuTra.Any(tr => tr.MaBanSao == ct.MaBanSao) &&
                             ct.MaPhieuMuon != excludePhieuMuonId)
                .Select(ct => ct.MaBanSao)
                .Distinct()
                .ToList();
        }


        public IEnumerable<BanSaoSach> GetAvailableBanSaoSach()
        {
            return dbService.DbContext.DsBanSaoSach
                .AsNoTracking()
                .Include(bs => bs.Sach)
                .Where(bs => bs.TinhTrang == "Có sẵn")
                .ToList();
        }

        public IEnumerable<BanSaoSach> GetAllBanSaoSach()
        {
            return dbService.DbContext.DsBanSaoSach
                .AsNoTracking()
                .Include(bs => bs.Sach)
                .ToList();
        }
        #endregion
    }
}