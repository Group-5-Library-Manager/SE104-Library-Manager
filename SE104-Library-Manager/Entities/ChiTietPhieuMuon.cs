﻿using System.ComponentModel.DataAnnotations.Schema;

namespace SE104_Library_Manager.Entities;

public class ChiTietPhieuMuon
{
    public required int MaPhieuMuon { get; set; }
    public required int MaSach { get; set; }

    [ForeignKey("MaPhieuMuon")]
    public PhieuMuon PhieuMuon { get; set; } = null!;

    [ForeignKey("MaSach")]
    public Sach Sach { get; set; } = null!;
}
