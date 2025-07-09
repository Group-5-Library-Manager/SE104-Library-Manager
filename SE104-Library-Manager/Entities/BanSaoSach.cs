using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class BanSaoSach
{
    [Key]
    public int MaBanSao { get; set; }

    [Required]
    public int MaSach { get; set; }

    [Required]
    public string TinhTrang { get; set; } = string.Empty;

    [ForeignKey(nameof(MaSach))]
    public Sach Sach { get; set; } = null!;

    // Navigation properties for collections
    public ICollection<ChiTietPhieuMuon> DsChiTietPhieuMuon { get; set; } = new List<ChiTietPhieuMuon>();
    public ICollection<ChiTietPhieuTra> DsChiTietPhieuTra { get; set; } = new List<ChiTietPhieuTra>();
} 