using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class Added_columns_In_TaskRequest_And_Vendoe_tbls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PropertyId",
                table: "Vendor");

            migrationBuilder.RenameColumn(
                name: "UnitIds",
                table: "Vendor",
                newName: "PolicyNumber");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Vendor",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceCompany",
                table: "Vendor",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "TaskRequest",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskRequest_UnitId",
                table: "TaskRequest",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskRequest_AssetsUnits_UnitId",
                table: "TaskRequest",
                column: "UnitId",
                principalTable: "AssetsUnits",
                principalColumn: "UnitId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskRequest_AssetsUnits_UnitId",
                table: "TaskRequest");

            migrationBuilder.DropIndex(
                name: "IX_TaskRequest_UnitId",
                table: "TaskRequest");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "InsuranceCompany",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "TaskRequest");

            migrationBuilder.RenameColumn(
                name: "PolicyNumber",
                table: "Vendor",
                newName: "UnitIds");

            migrationBuilder.AddColumn<int>(
                name: "PropertyId",
                table: "Vendor",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
