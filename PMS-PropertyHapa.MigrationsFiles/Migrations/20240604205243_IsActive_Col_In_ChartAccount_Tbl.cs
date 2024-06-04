using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class IsActive_Col_In_ChartAccount_Tbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ChartAccount",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ChartAccount");
        }
    }
}
