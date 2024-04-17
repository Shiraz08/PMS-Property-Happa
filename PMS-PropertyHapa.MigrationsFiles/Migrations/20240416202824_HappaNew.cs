using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class HappaNew : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BandwidthGB",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiskSpaceGB",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Domains",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmailAccounts",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Subdomains",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BandwidthGB",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DiskSpaceGB",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Domains",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "EmailAccounts",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Subdomains",
                table: "Subscriptions");
        }
    }
}
