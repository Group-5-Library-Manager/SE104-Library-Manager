using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE104_Library_Manager.Entities
{
    public class ChiTietPhieuNhap
    {
        public required int MaPhieuNhap { get; set; }
        public required int MaSach { get; set; }

        public int SoLuong { get; set; }
        public int DonGiaNhap { get; set; } // Giá mua thực tế
        public int ThanhTien => SoLuong * DonGiaNhap;

        [ForeignKey("MaPhieuNhap")]
        public PhieuNhap PhieuNhap { get; set; } = null!;

        [ForeignKey("MaSach")]
        public Sach Sach { get; set; } = null!;
    }

}
