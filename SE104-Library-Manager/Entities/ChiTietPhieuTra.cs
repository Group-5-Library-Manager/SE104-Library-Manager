using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class ChiTietPhieuTra
{
    public required int MaPhieuTra { get; set; }
    public required int MaSach { get; set; }
    public required int MaPhieuMuon { get; set; }
    public int TienPhat { get; set; } = 0;
    public bool DaXoa { get; set; } = false;

    [ForeignKey("MaPhieuTra")]
    public PhieuTra PhieuTra { get; set; } = null!;

    [ForeignKey("MaSach")]
    public Sach Sach { get; set; } = null!;

    [ForeignKey("MaPhieuMuon")]
    public PhieuMuon PhieuMuon { get; set; } = null!;

}
