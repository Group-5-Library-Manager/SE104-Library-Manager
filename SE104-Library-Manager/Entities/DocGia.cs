using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class DocGia
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaDocGia { get; set; }
    public required string TenDocGia { get; set; }
    public required string DiaChi { get; set; }
    public string Email { get; set; } = string.Empty;
    public required int MaLoaiDocGia { get; set; }
    public required DateOnly NgaySinh { get; set; }
    public required DateOnly NgayLapThe { get; set; }
    public int TongNo { get; set; } = 0;
    public bool DaXoa { get; set; } = false;

    [ForeignKey("MaLoaiDocGia")]
    public LoaiDocGia LoaiDocGia { get; set; } = null!;

    public ICollection<PhieuMuon> DsPhieuMuon { get; set; } = new List<PhieuMuon>();

    public ICollection<PhieuTra> DsPhieuTra { get; set; } = new List<PhieuTra>();
    
    public ICollection<PhieuPhat> DsPhieuPhat { get; set; } = new List<PhieuPhat>();
}
