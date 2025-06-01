using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class PhieuMuon
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaPhieuMuon { get; set; }
    public required DateOnly NgayMuon { get; set; }
    public required int MaDocGia { get; set; }
    public required int MaNhanVien { get; set; }
    public bool DaXoa { get; set; } = false;

    [ForeignKey("MaNhanVien")]
    public NhanVien NhanVien { get; set; } = null!;

    [ForeignKey("MaDocGia")]
    public DocGia DocGia { get; set; } = null!;

    public ICollection<ChiTietPhieuMuon> DsChiTietPhieuMuon { get; set; } = new List<ChiTietPhieuMuon>();

    public ICollection<ChiTietPhieuTra> DsChiTietPhieuTra { get; set; } = new List<ChiTietPhieuTra>();
}
