using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogService.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoaiDanhMucs",
                columns: table => new
                {
                    MaLDM = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenLDM = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiDanhMucs", x => x.MaLDM);
                });

            migrationBuilder.CreateTable(
                name: "DanhMucs",
                columns: table => new
                {
                    MaDM = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaLDM = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenDM = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucs", x => x.MaDM);
                    table.ForeignKey(
                        name: "FK_DanhMucs_LoaiDanhMucs_MaLDM",
                        column: x => x.MaLDM,
                        principalTable: "LoaiDanhMucs",
                        principalColumn: "MaLDM",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SanPhams",
                columns: table => new
                {
                    MaSP = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaDM = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenSP = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhams", x => x.MaSP);
                    table.ForeignKey(
                        name: "FK_SanPhams_DanhMucs_MaDM",
                        column: x => x.MaDM,
                        principalTable: "DanhMucs",
                        principalColumn: "MaDM",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietSanPhams",
                columns: table => new
                {
                    MaCTSP = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaSP = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Mau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KichCo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    Anh = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietSanPhams", x => x.MaCTSP);
                    table.ForeignKey(
                        name: "FK_ChiTietSanPhams_SanPhams_MaSP",
                        column: x => x.MaSP,
                        principalTable: "SanPhams",
                        principalColumn: "MaSP",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietSanPhams_MaSP",
                table: "ChiTietSanPhams",
                column: "MaSP");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucs_MaLDM",
                table: "DanhMucs",
                column: "MaLDM");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_MaDM",
                table: "SanPhams",
                column: "MaDM");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietSanPhams");

            migrationBuilder.DropTable(
                name: "SanPhams");

            migrationBuilder.DropTable(
                name: "DanhMucs");

            migrationBuilder.DropTable(
                name: "LoaiDanhMucs");
        }
    }
}
