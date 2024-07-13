using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class Added_LateFee_Tbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Classification",
                table: "Vendor",
                newName: "VendorClassificationIds");

            migrationBuilder.AddColumn<string>(
                name: "CalculateFee",
                table: "FeeCharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ChargeLatefeeActive",
                table: "FeeCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ChartAccountId",
                table: "FeeCharge",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DueDays",
                table: "FeeCharge",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "FeeCharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsChargeLateFee",
                table: "FeeCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsChargeLateFeeonSpecific",
                table: "FeeCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDailyLimit",
                table: "FeeCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnableSms",
                table: "FeeCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMinimumBalance",
                table: "FeeCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMonthlyLimit",
                table: "FeeCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNotifyTenants",
                table: "FeeCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSendARemainder",
                table: "FeeCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SpecifyLateFeeStructure",
                table: "FeeCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UsePropertyDefaultStructure",
                table: "FeeCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "LateFeeAsset",
                columns: table => new
                {
                    LateFeeAssetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    CompanyDefaultStructure = table.Column<bool>(type: "bit", nullable: false),
                    SpecifyLateFeeStructure = table.Column<bool>(type: "bit", nullable: false),
                    DueDays = table.Column<int>(type: "int", nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CalculateFee = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ChartAccountId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSendARemainder = table.Column<bool>(type: "bit", nullable: false),
                    IsNotifyTenants = table.Column<bool>(type: "bit", nullable: false),
                    IsEnableSms = table.Column<bool>(type: "bit", nullable: false),
                    IsChargeLateFee = table.Column<bool>(type: "bit", nullable: false),
                    IsMonthlyLimit = table.Column<bool>(type: "bit", nullable: false),
                    IsDailyLimit = table.Column<bool>(type: "bit", nullable: false),
                    IsMinimumBalance = table.Column<bool>(type: "bit", nullable: false),
                    IsChargeLateFeeonSpecific = table.Column<bool>(type: "bit", nullable: false),
                    AppTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LateFeeAsset", x => x.LateFeeAssetId);
                });

            migrationBuilder.CreateTable(
                name: "LateFees",
                columns: table => new
                {
                    LateFeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DueDays = table.Column<int>(type: "int", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CalculateFee = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChartAccountId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSendARemainder = table.Column<bool>(type: "bit", nullable: false),
                    IsNotifyTenants = table.Column<bool>(type: "bit", nullable: false),
                    IsEnableSms = table.Column<bool>(type: "bit", nullable: false),
                    IsChargeLateFee = table.Column<bool>(type: "bit", nullable: false),
                    IsMonthlyLimit = table.Column<bool>(type: "bit", nullable: false),
                    IsDailyLimit = table.Column<bool>(type: "bit", nullable: false),
                    IsMinimumBalance = table.Column<bool>(type: "bit", nullable: false),
                    IsChargeLateFeeonSpecific = table.Column<bool>(type: "bit", nullable: false),
                    AppTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LateFees", x => x.LateFeeId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LateFeeAsset");

            migrationBuilder.DropTable(
                name: "LateFees");

            migrationBuilder.DropColumn(
                name: "CalculateFee",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "ChargeLatefeeActive",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "ChartAccountId",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "DueDays",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "IsChargeLateFee",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "IsChargeLateFeeonSpecific",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "IsDailyLimit",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "IsEnableSms",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "IsMinimumBalance",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "IsMonthlyLimit",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "IsNotifyTenants",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "IsSendARemainder",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "SpecifyLateFeeStructure",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "UsePropertyDefaultStructure",
                table: "FeeCharge");

            migrationBuilder.RenameColumn(
                name: "VendorClassificationIds",
                table: "Vendor",
                newName: "Classification");
        }
    }
}
