namespace SE104_Library_Manager.Models;

public record UserProfile
{
    public required int MaNhanVien { get; init; }
    public required string TenDangNhap{ get; init; }
    public required string TenNhanVien{ get; init; }
    public required string DienThoai{ get; init; }
    public required DateOnly NgaySinh{ get; init; }
    public required string Role{ get; init; }
}
