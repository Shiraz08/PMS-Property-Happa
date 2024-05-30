using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class update_Applications_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Agree",
                table: "Applications");

            migrationBuilder.AddColumn<bool>(
                name: "IsAgree",
                table: "Applications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAgree",
                table: "Applications");

            migrationBuilder.AddColumn<string>(
                name: "Agree",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
