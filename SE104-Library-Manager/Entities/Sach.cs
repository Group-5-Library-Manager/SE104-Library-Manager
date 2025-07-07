
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class Sach
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaSach { get; set; }
    public required string TenSach { get; set; }
    public required int MaTheLoai { get; set; }
    public required int MaTacGia { get; set; }
    public required int NamXuatBan { get; set; }
    public required int MaNhaXuatBan { get; set; }
    public required DateOnly NgayNhap { get; set; }
    public required int TriGia { get; set; }
    public required string TrangThai { get; set; }
    public int SoLuongHienCo { get; set; } = 0;
    public int SoLuongTong { get; set; } = 0;
    public bool DaXoa { get; set; } = false;

    [ForeignKey("MaTheLoai")]
    public TheLoai TheLoai { get; set; } = null!;

    [ForeignKey("MaTacGia")]
    public TacGia TacGia { get; set; } = null!;

    [ForeignKey("MaNhaXuatBan")]
    public NhaXuatBan NhaXuatBan { get; set; } = null!;

    public ICollection<ChiTietPhieuMuon> DsChiTietPhieuMuon { get; set; } = new List<ChiTietPhieuMuon>();

    public ICollection<ChiTietPhieuTra> DsChiTietPhieuTra { get; set; } = new List<ChiTietPhieuTra>();

    public ICollection<ChiTietPhieuNhap> DsChiTietPhieuNhap { get; set; } = new List<ChiTietPhieuNhap>();
}
