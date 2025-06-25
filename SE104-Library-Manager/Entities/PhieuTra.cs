using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class PhieuTra
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaPhieuTra { get; set; }
    public required DateOnly NgayTra { get; set; }
    public int TienPhatKyNay { get; set; } = 0;
    public int TongNo { get; set; } = 0;
    public required int MaNhanVien { get; set; }
    public required int MaDocGia { get; set; }
    public bool DaXoa { get; set; } = false;

    [ForeignKey("MaNhanVien")]
    public NhanVien NhanVien { get; set; } = null!;

    [ForeignKey("MaDocGia")]
    public DocGia DocGia { get; set; } = null!;

    public ICollection<ChiTietPhieuTra> DsChiTietPhieuTra { get; set; } = new List<ChiTietPhieuTra>();
}
