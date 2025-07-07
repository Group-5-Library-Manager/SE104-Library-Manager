using SE104_Library_Manager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE104_Library_Manager.Interfaces.Repositories
{
    public interface IPhieuNhapRepository
    {
        Task<PhieuNhap> TaoPhieuNhapAsync(PhieuNhap phieuNhap, List<ChiTietPhieuNhap> dsChiTiet);
    }
}
