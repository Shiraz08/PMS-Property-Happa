using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class Updated_TaskRequest_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskRequest_Assets_AssetId",
                table: "TaskRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskRequest_Owner_OwnerId",
                table: "TaskRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskRequest_Tenant_TenantId",
                table: "TaskRequest");

            migrationBuilder.AlterColumn<int>(
                name: "VendorId",
                table: "TaskRequest",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "TaskRequest",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "TaskRequest",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AssetId",
                table: "TaskRequest",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Account",
                table: "LineItem",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskRequest_Assets_AssetId",
                table: "TaskRequest",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "AssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskRequest_Owner_OwnerId",
                table: "TaskRequest",
                column: "OwnerId",
                principalTable: "Owner",
                principalColumn: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskRequest_Tenant_TenantId",
                table: "TaskRequest",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskRequest_Assets_AssetId",
                table: "TaskRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskRequest_Owner_OwnerId",
                table: "TaskRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskRequest_Tenant_TenantId",
                table: "TaskRequest");

            migrationBuilder.AlterColumn<int>(
                name: "VendorId",
                table: "TaskRequest",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "TaskRequest",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "TaskRequest",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AssetId",
                table: "TaskRequest",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Account",
                table: "LineItem",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskRequest_Assets_AssetId",
                table: "TaskRequest",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "AssetId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskRequest_Owner_OwnerId",
                table: "TaskRequest",
                column: "OwnerId",
                principalTable: "Owner",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskRequest_Tenant_TenantId",
                table: "TaskRequest",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
