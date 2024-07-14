using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class Added_LateFee_Column : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LatefeeChargeAmount",
                table: "RecurringJobs",
                newName: "LateFeeChargeAmount");

            migrationBuilder.RenameColumn(
                name: "LatefeePropertyId",
                table: "RecurringJobs",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "AgreementFormId",
                table: "RecurringJobs",
                newName: "LeaseId");

            migrationBuilder.RenameColumn(
                name: "RecurringJobs_Id",
                table: "RecurringJobs",
                newName: "RecurringJobsId");

            migrationBuilder.AddColumn<string>(
                name: "CalculateFee",
                table: "RecurringJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DueDaysProcess",
                table: "RecurringJobs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "RecurringJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LateFeeAssetId",
                table: "RecurringJobs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LateFeeLeaseId",
                table: "RecurringJobs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ChargeLateFeeActive",
                table: "LateFees",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalculateFee",
                table: "RecurringJobs");

            migrationBuilder.DropColumn(
                name: "DueDaysProcess",
                table: "RecurringJobs");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "RecurringJobs");

            migrationBuilder.DropColumn(
                name: "LateFeeAssetId",
                table: "RecurringJobs");

            migrationBuilder.DropColumn(
                name: "LateFeeLeaseId",
                table: "RecurringJobs");

            migrationBuilder.DropColumn(
                name: "ChargeLateFeeActive",
                table: "LateFees");

            migrationBuilder.RenameColumn(
                name: "LateFeeChargeAmount",
                table: "RecurringJobs",
                newName: "LatefeeChargeAmount");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "RecurringJobs",
                newName: "LatefeePropertyId");

            migrationBuilder.RenameColumn(
                name: "LeaseId",
                table: "RecurringJobs",
                newName: "AgreementFormId");

            migrationBuilder.RenameColumn(
                name: "RecurringJobsId",
                table: "RecurringJobs",
                newName: "RecurringJobs_Id");
        }
    }
}
