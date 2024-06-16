using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class add_TaskRequest_rowChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountBank",
                table: "TaskRequest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountCurrency",
                table: "TaskRequest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountHolder",
                table: "TaskRequest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountIBAN",
                table: "TaskRequest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountName",
                table: "TaskRequest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountSwift",
                table: "TaskRequest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentFile",
                table: "TaskRequest",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountBank",
                table: "TaskRequest");

            migrationBuilder.DropColumn(
                name: "AccountCurrency",
                table: "TaskRequest");

            migrationBuilder.DropColumn(
                name: "AccountHolder",
                table: "TaskRequest");

            migrationBuilder.DropColumn(
                name: "AccountIBAN",
                table: "TaskRequest");

            migrationBuilder.DropColumn(
                name: "AccountName",
                table: "TaskRequest");

            migrationBuilder.DropColumn(
                name: "AccountSwift",
                table: "TaskRequest");

            migrationBuilder.DropColumn(
                name: "DocumentFile",
                table: "TaskRequest");
        }
    }
}
