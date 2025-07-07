using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;

namespace SE104_Library_Manager.Data;

public class DatabaseContext(DbContextOptions options) : DbContext(options)
{
    #region DbSets - Entity Collections
    public DbSet<BangCap> DsBangCap { get; set; } = null!;
    public DbSet<BoPhan> DsBoPhan { get; set; } = null!;
    public DbSet<ChiTietPhieuMuon> DsChiTietPhieuMuon { get; set; } = null!;
    public DbSet<ChiTietPhieuNhap> DsChiTietPhieuNhap { get; set; } = null!;
    public DbSet<ChiTietPhieuTra> DsChiTietPhieuTra { get; set; } = null!;
    public DbSet<ChucVu> DsChucVu { get; set; } = null!;
    public DbSet<DocGia> DsDocGia { get; set; } = null!;
    public DbSet<LoaiDocGia> DsLoaiDocGia { get; set; } = null!;
    public DbSet<NhanVien> DsNhanVien { get; set; } = null!;
    public DbSet<NhaXuatBan> DsNhaXuatBan { get; set; } = null!;
    public DbSet<PhieuMuon> DsPhieuMuon { get; set; } = null!;
    public DbSet<PhieuTra> DsPhieuTra { get; set; } = null!;
    public DbSet<PhieuPhat> DsPhieuPhat { get; set; } = null!;
    public DbSet<PhieuNhap> DsPhieuNhap { get; set; } = null!;
    public DbSet<QuyDinh> DsQuyDinh { get; set; } = null!;
    public DbSet<Sach> DsSach { get; set; } = null!;
    public DbSet<TacGia> DsTacGia { get; set; } = null!;
    public DbSet<TaiKhoan> DsTaiKhoan { get; set; } = null!;
    public DbSet<TheLoai> DsTheLoai { get; set; } = null!;
    public DbSet<VaiTro> DsVaiTro { get; set; } = null!;
    #endregion
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureCompositeKeys(modelBuilder);

        ConfigureRestrictDeleteBehavior(modelBuilder); // Prevent deletion of parent records if child records exist
        
        ConfigureCascadeDeleteBehavior(modelBuilder); // Delete child records when parent is deleted

        base.OnModelCreating(modelBuilder);
    }

    #region Configuration Methods
    private static void ConfigureCompositeKeys(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTietPhieuMuon>()
            .HasKey(c => new {
                c.MaPhieuMuon,
                c.MaSach
            });

        modelBuilder.Entity<ChiTietPhieuNhap>()
            .HasKey(c => new {
                c.MaPhieuNhap,
                c.MaSach
            });

        modelBuilder.Entity<ChiTietPhieuTra>()
            .HasKey(c => new
            {
                c.MaPhieuTra,
                c.MaSach,
            });
    }

    private static void ConfigureRestrictDeleteBehavior(ModelBuilder modelBuilder)
    {
        ConfigureEmployeeRestrictions(modelBuilder);
        ConfigureReaderRestrictions(modelBuilder);
        ConfigureBookRestrictions(modelBuilder);
        ConfigureTransactionRestrictions(modelBuilder);
    }

    private static void ConfigureEmployeeRestrictions(ModelBuilder modelBuilder)
    {
        // NhanVien -> BangCap
        modelBuilder.Entity<BangCap>()
            .HasMany(bc => bc.DsNhanVien)
            .WithOne(nv => nv.BangCap)
            .HasForeignKey(nv => nv.MaBangCap)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);

        // NhanVien -> BoPhan
        modelBuilder.Entity<BoPhan>()
            .HasMany(bp => bp.DsNhanVien)
            .WithOne(nv => nv.BoPhan)
            .HasForeignKey(nv => nv.MaBoPhan)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);

        // NhanVien -> ChucVu
        modelBuilder.Entity<ChucVu>()
            .HasMany(cv => cv.DsNhanVien)
            .WithOne(nv => nv.ChucVu)
            .HasForeignKey(nv => nv.MaChucVu)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);

        // TaiKhoan -> VaiTro
        modelBuilder.Entity<VaiTro>()
            .HasMany(vt => vt.DsTaiKhoan)
            .WithOne(tk => tk.VaiTro)
            .HasForeignKey(tk => tk.MaVaiTro)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);

        // PhieuMuon -> NhanVien
        modelBuilder.Entity<NhanVien>()
            .HasMany(nv => nv.DsPhieuMuon)
            .WithOne(pm => pm.NhanVien)
            .HasForeignKey(pm => pm.MaNhanVien)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);

        // PhieuTra -> NhanVien
        modelBuilder.Entity<NhanVien>()
            .HasMany(nv => nv.DsPhieuTra)
            .WithOne(pt => pt.NhanVien)
            .HasForeignKey(pt => pt.MaNhanVien)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureReaderRestrictions(ModelBuilder modelBuilder)
    {
        // DocGia -> LoaiDocGia
        modelBuilder.Entity<LoaiDocGia>()
            .HasMany(ldg => ldg.DsDocGia)
            .WithOne(dg => dg.LoaiDocGia)
            .HasForeignKey(dg => dg.MaLoaiDocGia)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);

        // PhieuMuon -> DocGia
        modelBuilder.Entity<DocGia>()
            .HasMany(dg => dg.DsPhieuMuon)
            .WithOne(pm => pm.DocGia)
            .HasForeignKey(pm => pm.MaDocGia)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);

        // PhieuTra -> DocGia
        modelBuilder.Entity<DocGia>()
            .HasMany(dg => dg.DsPhieuTra)
            .WithOne(pt => pt.DocGia)
            .HasForeignKey(pt => pt.MaDocGia)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);

        // PhieuPhat -> DocGia
        modelBuilder.Entity<DocGia>()
            .HasMany(dg => dg.DsPhieuPhat)
            .WithOne(pt => pt.DocGia)
            .HasForeignKey(pt => pt.MaDocGia)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureBookRestrictions(ModelBuilder modelBuilder)
    {

        // Sach -> TheLoai
        modelBuilder.Entity<TheLoai>()
            .HasMany(tl => tl.DsSach)
            .WithOne(s => s.TheLoai)
            .HasForeignKey(s => s.MaTheLoai)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);

        // Sach -> TacGia
        modelBuilder.Entity<TacGia>()
            .HasMany(tg => tg.DsSach)
            .WithOne(s => s.TacGia)
            .HasForeignKey(s => s.MaTacGia)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);

        // Sach -> NhaXuatBan
        modelBuilder.Entity<NhaXuatBan>()
            .HasMany(nxb => nxb.DsSach)
            .WithOne(s => s.NhaXuatBan)
            .HasForeignKey(s => s.MaNhaXuatBan)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);

        // ChiTietPhieuMuon -> Sach
        modelBuilder.Entity<Sach>()
            .HasMany(s => s.DsChiTietPhieuMuon)
            .WithOne(ctpm => ctpm.Sach)
            .HasForeignKey(ctpm => ctpm.MaSach)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);

        // ChiTietPhieuTra -> Sach
        modelBuilder.Entity<Sach>()
            .HasMany(s => s.DsChiTietPhieuTra)
            .WithOne(ctpt => ctpt.Sach)
            .HasForeignKey(ctpt => ctpt.MaSach)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);

        // ChiTietPhieuNhap -> Sach
        modelBuilder.Entity<Sach>()
            .HasMany(s => s.DsChiTietPhieuNhap)
            .WithOne(ctpn => ctpn.Sach)
            .HasForeignKey(ctpn => ctpn.MaSach)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureTransactionRestrictions(ModelBuilder modelBuilder)
    {
        // ChiTietPhieuTra -> PhieuMuon
        modelBuilder.Entity<PhieuMuon>()
            .HasMany(pt => pt.DsChiTietPhieuTra)
            .WithOne(ctpt => ctpt.PhieuMuon)
            .HasForeignKey(ctpt => ctpt.MaPhieuMuon)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureCascadeDeleteBehavior(ModelBuilder modelBuilder)
    {
        // TaiKhoan -> NhanVien
        modelBuilder.Entity<NhanVien>()
            .HasOne(nv => nv.TaiKhoan)
            .WithOne(tk => tk.NhanVien)
            .HasForeignKey<TaiKhoan>(tk => tk.MaNhanVien)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Cascade);

        // ChiTietPhieuMuon -> PhieuMuon
        modelBuilder.Entity<PhieuMuon>()
            .HasMany(pm => pm.DsChiTietPhieuMuon)
            .WithOne(ctpm => ctpm.PhieuMuon)
            .HasForeignKey(ctpm => ctpm.MaPhieuMuon)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Cascade);

        // ChiTietPhieuTra -> PhieuTra
        modelBuilder.Entity<PhieuTra>()
            .HasMany(pt => pt.DsChiTietPhieuTra)
            .WithOne(ctpt => ctpt.PhieuTra)
            .HasForeignKey(ctpt => ctpt.MaPhieuTra)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Cascade);

        // ChiTietPhieuNhap -> PhieuNhap
        modelBuilder.Entity<PhieuNhap>()
            .HasMany(pn => pn.DsChiTietPhieuNhap)
            .WithOne(ctpn => ctpn.PhieuNhap)
            .HasForeignKey(ctpn => ctpn.MaPhieuNhap)
            .IsRequired() // Ensure foreign key is required
            .OnDelete(DeleteBehavior.Cascade);
    }
    #endregion
}