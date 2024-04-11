using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class tiwilo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountSid",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthToken",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TiwiloPhone",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountSid",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AuthToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TiwiloPhone",
                table: "AspNetUsers");
        }
    }
}
