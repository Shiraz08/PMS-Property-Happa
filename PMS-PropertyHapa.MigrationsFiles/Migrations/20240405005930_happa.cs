using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class happa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lease_Tenant_TenantsTenantId",
                table: "Lease");

            migrationBuilder.AddColumn<DateTime>(
                name: "RentDate",
                table: "RentCharge",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RentPeriod",
                table: "RentCharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TenantsTenantId",
                table: "Lease",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Lease_Tenant_TenantsTenantId",
                table: "Lease",
                column: "TenantsTenantId",
                principalTable: "Tenant",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lease_Tenant_TenantsTenantId",
                table: "Lease");

            migrationBuilder.DropColumn(
                name: "RentDate",
                table: "RentCharge");

            migrationBuilder.DropColumn(
                name: "RentPeriod",
                table: "RentCharge");

            migrationBuilder.AlterColumn<int>(
                name: "TenantsTenantId",
                table: "Lease",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Lease_Tenant_TenantsTenantId",
                table: "Lease",
                column: "TenantsTenantId",
                principalTable: "Tenant",
                principalColumn: "TenantId");
        }
    }
}
