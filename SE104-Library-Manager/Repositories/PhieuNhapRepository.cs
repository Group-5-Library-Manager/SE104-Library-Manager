using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Data;
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
    public class PhieuNhapRepository : IPhieuNhapRepository
    {
        private readonly DatabaseService _dbService;

        public PhieuNhapRepository(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<List<PhieuNhap>> GetAllAsync()
        {
            return await _dbService.DbContext.DsPhieuNhap
                .Include(p => p.NhanVien)
                .Include(p => p.DsChiTietPhieuNhap)
                    .ThenInclude(ct => ct.Sach)
                .Where(p => !p.DaXoa)
                .OrderBy(p => p.MaPhieuNhap)
                .ToListAsync();
        }

        public async Task<PhieuNhap> TaoPhieuNhapAsync(PhieuNhap phieuNhap, List<ChiTietPhieuNhap> dsChiTiet)
        {
            // Gắn danh sách chi tiết vào phiếu
            phieuNhap.DsChiTietPhieuNhap = dsChiTiet;

            // Tính tổng tiền & tổng số lượng
            phieuNhap.TongSoLuong = dsChiTiet.Sum(ct => ct.SoLuong);
            phieuNhap.TongTien = dsChiTiet.Sum(ct => ct.SoLuong * ct.DonGiaNhap);

            // Cập nhật số lượng sách
            foreach (var ct in dsChiTiet)
            {
                var sach = await _dbService.DbContext.DsSach.FindAsync(ct.MaSach);
                if (sach == null)
                    throw new InvalidOperationException($"Không tìm thấy sách có mã {ct.MaSach}");

                sach.SoLuongHienCo += ct.SoLuong;
                sach.SoLuongTong += ct.SoLuong;
                SE104_Library_Manager.Repositories.SachRepository.UpdateBookStatus(sach);

                // Thêm các bản sao sách
                for (int i = 0; i < ct.SoLuong; i++)
                {
                    var banSao = new BanSaoSach
                    {
                        MaSach = ct.MaSach,
                        TinhTrang = "Mới nhập"
                    };
                    _dbService.DbContext.DsBanSaoSach.Add(banSao);
                }
            }

            _dbService.DbContext.DsPhieuNhap.Add(phieuNhap);
            await _dbService.DbContext.SaveChangesAsync();

            return phieuNhap;
        }
    }
}
