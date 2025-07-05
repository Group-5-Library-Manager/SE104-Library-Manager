using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class ChucVu
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaChucVu { get; set; }
    public required string TenChucVu { get; set; }
    public bool DaXoa { get; set; } = false;

    public ICollection<NhanVien> DsNhanVien { get; set; } = new List<NhanVien>();
}
