using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SE104_Library_Manager.Entities;
public class PhieuPhat
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaPhieuPhat { get; set; }
    public required DateOnly NgayLap { get; set; }
    public required int MaDocGia { get; set; }
    public int TongNo { get; set; } = 0;
    public int TienThu { get; set; } = 0;
    public int ConLai { get; set; } = 0;

    [ForeignKey("MaDocGia")]
    public DocGia DocGia { get; set; } = null!;
    public bool DaXoa { get; set; } = false;
}
