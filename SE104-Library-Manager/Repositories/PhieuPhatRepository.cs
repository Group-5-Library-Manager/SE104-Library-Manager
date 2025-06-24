using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;
using System.IO;

namespace SE104_Library_Manager.Repositories;

public class PhieuPhatRepository(DatabaseService db) : IPhieuPhatRepository
{
    public async Task<List<PhieuPhat>> GetAllAsync()
    {
        return await db.DbContext.DsPhieuPhat
            .Include(pp => pp.DocGia)
            .Where(pp => !pp.DaXoa)
            .ToListAsync();
    }

    public async Task<PhieuPhat?> GetByIdAsync(int maPhieuPhat)
    {
        return await db.DbContext.DsPhieuPhat
            .Include(pp => pp.DocGia)
            .FirstOrDefaultAsync(pp => pp.MaPhieuPhat == maPhieuPhat && !pp.DaXoa);
    }
    public async Task<List<DocGia>> GetReadersWithDebtAsync()
    {
        return await db.DbContext.DsDocGia
            .Where(dg => !dg.DaXoa && dg.TongNo > 0)
            .ToListAsync();
    }

    public async Task AddAsync(PhieuPhat phieuPhat)
    {
        await db.DbContext.DsPhieuPhat.AddAsync(phieuPhat);
        await db.DbContext.SaveChangesAsync();
        db.DbContext.ChangeTracker.Clear();
    }

    public async Task DeleteAsync(int maPhieuPhat)
    {
        var existing = await db.DbContext.DsPhieuPhat.FindAsync(maPhieuPhat);
        if (existing != null)
        {
            existing.DaXoa = true;
            await db.DbContext.SaveChangesAsync();
            db.DbContext.ChangeTracker.Clear();
        }
    }

    public async Task<bool> ExportAsync(PhieuPhat phieuPhat)
    {
        var docGia = await db.DbContext.DsDocGia.FindAsync(phieuPhat.MaDocGia);

        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "PDF Files|*.pdf",
            FileName = $"PhieuPhat_PP{phieuPhat.MaPhieuPhat}.pdf"
        };
        if (saveFileDialog.ShowDialog() != true)
        {
            return false;
        }

        var exportPath = saveFileDialog.FileName;

        using (var writer = new PdfWriter(exportPath))
        {
            var pdf = new PdfDocument(writer);
            var doc = new iText.Layout.Document(pdf);

            // Tải font Unicode
            var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
            var font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);

            // Tiêu đề
            var title = new Paragraph("PHIẾU PHẠT THƯ VIỆN")
                .SetFont(font).SetFontSize(18).SetBold()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
            doc.Add(title);
            doc.Add(new Paragraph("\n"));

            // Nội dung chi tiết
            void AddLine(string label, string value) =>
                doc.Add(new Paragraph($"{label}: {value}")
                    .SetFont(font).SetFontSize(12));

            AddLine("Mã phiếu", $"PP{phieuPhat.MaPhieuPhat}");
            AddLine("Ngày lập", $"{phieuPhat.NgayLap:dd/MM/yyyy}");
            AddLine("Mã độc giả", $"DG{phieuPhat.MaDocGia}");
            AddLine("Tên độc giả", $"{docGia?.TenDocGia ?? "N/A"}");
            AddLine("Tổng nợ", $"{phieuPhat.TongNo:N0} VND");
            AddLine("Số tiền thu", $"{phieuPhat.TienThu:N0} VND");
            AddLine("Còn lại", $"{phieuPhat.ConLai:N0} VND");

            doc.Add(new Paragraph("\nCảm ơn bạn đã sử dụng dịch vụ thư viện!")
                .SetFont(font).SetFontSize(10).SetItalic());

            doc.Close();
        }

        return true;
    }


}
