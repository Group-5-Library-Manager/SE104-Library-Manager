using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class VaiTro
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaVaiTro { get; set; }
    public required string TenVaiTro { get; set; }

    public ICollection<TaiKhoan> DsTaiKhoan { get; set; } = new List<TaiKhoan>();
}
