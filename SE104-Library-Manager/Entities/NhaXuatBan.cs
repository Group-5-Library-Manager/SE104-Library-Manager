using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class NhaXuatBan
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaNhaXuatBan { get; set; }
    public required string TenNhaXuatBan { get; set; }
    public bool DaXoa { get; set; } = false;

    public ICollection<Sach> DsSach { get; set; } = new List<Sach>();
}
