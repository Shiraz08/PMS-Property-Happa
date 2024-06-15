using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class Updated_columns_In_TaskRequest_And_Lease_tbls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Account",
                table: "LineItem",
                newName: "ChartAccountId");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "Lease",
                newName: "UnitId");

            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                table: "Lease",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LineItem_ChartAccountId",
                table: "LineItem",
                column: "ChartAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Lease_AssetId",
                table: "Lease",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Lease_UnitId",
                table: "Lease",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lease_Assets_AssetId",
                table: "Lease",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "AssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lease_AssetsUnits_UnitId",
                table: "Lease",
                column: "UnitId",
                principalTable: "AssetsUnits",
                principalColumn: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_LineItem_ChartAccount_ChartAccountId",
                table: "LineItem",
                column: "ChartAccountId",
                principalTable: "ChartAccount",
                principalColumn: "ChartAccountId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lease_Assets_AssetId",
                table: "Lease");

            migrationBuilder.DropForeignKey(
                name: "FK_Lease_AssetsUnits_UnitId",
                table: "Lease");

            migrationBuilder.DropForeignKey(
                name: "FK_LineItem_ChartAccount_ChartAccountId",
                table: "LineItem");

            migrationBuilder.DropIndex(
                name: "IX_LineItem_ChartAccountId",
                table: "LineItem");

            migrationBuilder.DropIndex(
                name: "IX_Lease_AssetId",
                table: "Lease");

            migrationBuilder.DropIndex(
                name: "IX_Lease_UnitId",
                table: "Lease");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "Lease");

            migrationBuilder.RenameColumn(
                name: "ChartAccountId",
                table: "LineItem",
                newName: "Account");

            migrationBuilder.RenameColumn(
                name: "UnitId",
                table: "Lease",
                newName: "PropertyId");
        }
    }
}
