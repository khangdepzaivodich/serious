using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderingService.Migrations
{
    public partial class AddCustomerInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HoTen",
                table: "DonHangs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "SoDienThoai",
                table: "DonHangs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: string.Empty);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoTen",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "SoDienThoai",
                table: "DonHangs");
        }
    }
}