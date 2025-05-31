using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class LoaiDocGia
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public required int MaLoaiDocGia { get; set; }
    public required string TenLoaiDocGia { get; set; }

    public ICollection<DocGia> DsDocGia { get; set; } = new List<DocGia>();
}
