using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderingService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DonHangs",
                columns: table => new
                {
                    MaDH = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaTK = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaGG = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiaChiGiaoHang = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThaiDH = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHangs", x => x.MaDH);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHangs",
                columns: table => new
                {
                    MaCTDH = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaDH = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaCTSP = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenSP_LuuTru = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mau_LuuTru = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KichCo_LuuTru = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gia_LuuTru = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHangs", x => x.MaCTDH);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_DonHangs_MaDH",
                        column: x => x.MaDH,
                        principalTable: "DonHangs",
                        principalColumn: "MaDH",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_MaDH",
                table: "ChiTietDonHangs",
                column: "MaDH");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonHangs");

            migrationBuilder.DropTable(
                name: "DonHangs");
        }
    }
}
