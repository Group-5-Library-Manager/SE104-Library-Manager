using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class NhanVien
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaNhanVien { get; set; }
    public required string TenNhanVien { get; set; }
    public required string DiaChi { get; set; }
    public required string DienThoai { get; set; }
    public required DateOnly NgaySinh { get; set; }
    public required int MaChucVu { get; set; }
    public required int MaBangCap { get; set; }
    public required int MaBoPhan { get; set; }
    public bool DaXoa { get; set; } = false;

    [ForeignKey("MaBangCap")]
    public BangCap BangCap { get; set; } = null!;

    [ForeignKey("MaBoPhan")]
    public BoPhan BoPhan { get; set; } = null!;

    [ForeignKey("MaChucVu")]
    public ChucVu ChucVu { get; set; } = null!;

    public TaiKhoan TaiKhoan { get; set; } = null!;

    public ICollection<PhieuMuon> DsPhieuMuon { get; set; } = new List<PhieuMuon>();
}
