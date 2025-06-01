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
    public required int MaDocGia { get; set; }
    public bool DaXoa { get; set; } = false;

    public DocGia DocGia { get; set; } = null!;

    public ICollection<ChiTietPhieuTra> DsChiTietPhieuTra { get; set; } = new List<ChiTietPhieuTra>();
}
