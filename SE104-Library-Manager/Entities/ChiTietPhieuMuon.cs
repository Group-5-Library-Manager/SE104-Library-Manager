using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class ChiTietPhieuMuon
{
    public int MaPhieuMuon { get; set; }
    [ForeignKey(nameof(MaPhieuMuon))]
    public PhieuMuon PhieuMuon { get; set; } = null!;

    public int MaBanSao { get; set; }
    [ForeignKey(nameof(MaBanSao))]
    public BanSaoSach BanSaoSach { get; set; } = null!;
}
