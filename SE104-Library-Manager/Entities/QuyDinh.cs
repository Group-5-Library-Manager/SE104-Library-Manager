using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class QuyDinh
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaQuyDinh { get; set; }
    public int TuoiDocGiaToiThieu { get; set; } = 18;
    public int TuoiDocGiaToiDa { get; set; } = 55;
    public int ThoiHanTheDocGia { get; set; } = 6; // Months
    public int SoNamXuatBanToiDa { get; set; } = 8; // Years
    public int SoNgayMuonToiDa { get; set; } = 30; // Days
    public int SoSachMuonToiDa { get; set; } = 5;
    public int TienPhatQuaHanMoiNgay { get; set; } = 1000; // VND
    public int SoTheLoaiToiDa { get; set; } = 3;
    public int SoTacGiaToiDa { get; set; } = 100;
    public int SoBoPhanToiDa { get; set; } = 4;
    public int SoBangCapToiDa { get; set; } = 5;
    public int SoChucVuToiDa { get; set; } = 5;
    public int SoLoaiDocGiaToiDa { get; set; } = 2;
}
