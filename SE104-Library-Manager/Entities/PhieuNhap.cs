using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE104_Library_Manager.Entities
{
    public class PhieuNhap
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaPhieuNhap { get; set; }

        public required int MaNhanVien { get; set; }
        public required DateOnly NgayNhap { get; set; }

        public int TongTien { get; set; } = 0;
        public int TongSoLuong { get; set; } = 0;
        public bool DaXoa { get; set; } = false;

        [ForeignKey("MaNhanVien")]
        public NhanVien NhanVien { get; set; } = null!;

        public ICollection<ChiTietPhieuNhap> DsChiTietPhieuNhap { get; set; } = new List<ChiTietPhieuNhap>();
    }

}
