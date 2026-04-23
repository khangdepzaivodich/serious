using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderingService.Migrations
{
    public partial class AddOrderItemImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Anh_LuuTru",
                table: "ChiTietDonHangs",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Anh_LuuTru",
                table: "ChiTietDonHangs");
        }
    }
}