using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Data;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;
using System;
using System.Collections.Generic;
using System.IO;
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

        public Task ExportToPdf(PhieuNhap phieuNhap)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                FileName = $"PhieuNhap_PN{phieuNhap.MaPhieuNhap}.pdf"
            };
            if (saveFileDialog.ShowDialog() != true)
            {
                return Task.CompletedTask;
            }

            var exportPath = saveFileDialog.FileName;

            using (var writer = new PdfWriter(exportPath))
            {
                var pdf = new PdfDocument(writer);
                var doc = new Document(pdf);

                // Tải font Unicode
                var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                var font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);

                // Tiêu đề
                var title = new Paragraph("PHIẾU NHẬP SÁCH")
                    .SetFont(font).SetFontSize(18).SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                doc.Add(title);
                doc.Add(new Paragraph("\n"));

                // Nội dung chi tiết
                void AddLine(string label, string value) =>
                    doc.Add(new Paragraph($"{label}: {value}")
                        .SetFont(font).SetFontSize(12));

                AddLine("Mã phiếu nhập", $"PN{phieuNhap.MaPhieuNhap}");
                AddLine("Ngày nhập", $"{phieuNhap.NgayNhap:dd/MM/yyyy}");
                AddLine("Nhân viên", $"{phieuNhap.NhanVien?.TenNhanVien ?? "N/A"}");
                AddLine("Tổng số lượng", $"{phieuNhap.TongSoLuong:N0}");
                AddLine("Tổng giá trị", $"{phieuNhap.TongTien:N0} VND");

                doc.Add(new Paragraph("\nChi tiết nhập sách:").SetFont(font).SetFontSize(13).SetBold());

                var table = new Table(5, false);
                table.AddHeaderCell(new Cell().Add(new Paragraph("STT").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Tên sách").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Số lượng").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Đơn giá").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Thành tiền").SetFont(font)));

                int stt = 1;
                foreach (var ct in phieuNhap.DsChiTietPhieuNhap)
                {
                    table.AddCell(new Cell().Add(new Paragraph(stt.ToString()).SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(ct.Sach?.TenSach ?? "").SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(ct.SoLuong.ToString()).SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(ct.DonGiaNhap.ToString("N0")).SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph((ct.SoLuong * ct.DonGiaNhap).ToString("N0")).SetFont(font)));
                    stt++;
                }
                doc.Add(table);

                doc.Add(new Paragraph("\nCảm ơn bạn đã sử dụng dịch vụ thư viện!")
                    .SetFont(font).SetFontSize(10).SetItalic());

                doc.Close();
            }

            return Task.CompletedTask;
        }
    }
}
