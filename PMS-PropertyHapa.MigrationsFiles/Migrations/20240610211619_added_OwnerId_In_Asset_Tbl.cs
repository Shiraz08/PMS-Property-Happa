using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class added_OwnerId_In_Asset_Tbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerAddress",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "OwnerCity",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "OwnerCompanyName",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "OwnerCountry",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "OwnerStreet",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "OwnerZipcode",
                table: "Assets");

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Assets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Assets");

            migrationBuilder.AddColumn<string>(
                name: "OwnerAddress",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerCity",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerCompanyName",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerCountry",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerStreet",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerZipcode",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
