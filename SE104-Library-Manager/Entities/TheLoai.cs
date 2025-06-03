using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class TheLoai
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaTheLoai { get; set; }
    public required string TenTheLoai { get; set; }

    public ICollection<Sach> DsSach { get; set; } = new List<Sach>();
}
