using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class Added_TenantOrganizationInfo_ColumnAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "TenantOrganizationInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "TenantOrganizationInfo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "AppTenantId",
                table: "TenantOrganizationInfo",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TenantOrganizationInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "TenantOrganizationInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "TenantOrganizationInfo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "TenantOrganizationInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "TenantOrganizationInfo");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "TenantOrganizationInfo");

            migrationBuilder.DropColumn(
                name: "AppTenantId",
                table: "TenantOrganizationInfo");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TenantOrganizationInfo");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "TenantOrganizationInfo");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "TenantOrganizationInfo");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TenantOrganizationInfo");
        }
    }
}
