using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class Added_AssetsUnits_ColumnAdded_up : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "AssetsUnits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "AssetsUnits",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "AppTenantId",
                table: "AssetsUnits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AssetsUnits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "AssetsUnits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "AssetsUnits",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "AssetsUnits",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "AssetsUnits");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "AssetsUnits");

            migrationBuilder.DropColumn(
                name: "AppTenantId",
                table: "AssetsUnits");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AssetsUnits");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "AssetsUnits");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "AssetsUnits");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AssetsUnits");
        }
    }
}
