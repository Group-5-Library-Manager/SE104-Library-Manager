using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class TaiKhoan
{
    [Key]
    public int MaNhanVien { get; set; }
    public required string TenDangNhap { get; set; }
    public required string MatKhau { get; set; }
    public required int MaVaiTro { get; set; }
    public bool DaXoa { get; set; } = false;

    [ForeignKey("MaNhanVien")]
    public NhanVien NhanVien { get; set; } = null!;

    [ForeignKey("MaVaiTro")]
    public VaiTro VaiTro { get; set; } = null!;
}
