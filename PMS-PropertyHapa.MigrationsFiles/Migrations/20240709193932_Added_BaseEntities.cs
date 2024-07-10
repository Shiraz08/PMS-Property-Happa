using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class Added_BaseEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "VendorOrganization",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "VendorOrganization",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "AppTenantId",
                table: "VendorOrganization",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "VendorOrganization",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "VendorOrganization",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "VendorOrganization",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "VendorOrganization",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "Vehicle",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "Vehicle",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "AppTenantId",
                table: "Vehicle",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Vehicle",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Vehicle",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "Vehicle",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Vehicle",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "SecurityDeposit",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "SecurityDeposit",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "AppTenantId",
                table: "SecurityDeposit",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SecurityDeposit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "SecurityDeposit",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "SecurityDeposit",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "SecurityDeposit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "RentCharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "RentCharge",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "AppTenantId",
                table: "RentCharge",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RentCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "RentCharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "RentCharge",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "RentCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "OwnerOrganization",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "OwnerOrganization",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "AppTenantId",
                table: "OwnerOrganization",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OwnerOrganization",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "OwnerOrganization",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "OwnerOrganization",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "OwnerOrganization",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "FeeCharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "FeeCharge",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "AppTenantId",
                table: "FeeCharge",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FeeCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "FeeCharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "FeeCharge",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "FeeCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "BudgetItemMonth",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "BudgetItemMonth",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "AppTenantId",
                table: "BudgetItemMonth",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BudgetItemMonth",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "BudgetItemMonth",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "BudgetItemMonth",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "BudgetItemMonth",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "VendorOrganization");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "VendorOrganization");

            migrationBuilder.DropColumn(
                name: "AppTenantId",
                table: "VendorOrganization");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "VendorOrganization");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "VendorOrganization");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "VendorOrganization");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "VendorOrganization");

            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "AppTenantId",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "SecurityDeposit");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "SecurityDeposit");

            migrationBuilder.DropColumn(
                name: "AppTenantId",
                table: "SecurityDeposit");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SecurityDeposit");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "SecurityDeposit");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "SecurityDeposit");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SecurityDeposit");

            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "RentCharge");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "RentCharge");

            migrationBuilder.DropColumn(
                name: "AppTenantId",
                table: "RentCharge");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RentCharge");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "RentCharge");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "RentCharge");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RentCharge");

            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "OwnerOrganization");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "OwnerOrganization");

            migrationBuilder.DropColumn(
                name: "AppTenantId",
                table: "OwnerOrganization");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OwnerOrganization");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "OwnerOrganization");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "OwnerOrganization");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OwnerOrganization");

            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "AppTenantId",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "FeeCharge");

            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "BudgetItemMonth");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "BudgetItemMonth");

            migrationBuilder.DropColumn(
                name: "AppTenantId",
                table: "BudgetItemMonth");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BudgetItemMonth");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "BudgetItemMonth");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "BudgetItemMonth");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "BudgetItemMonth");
        }
    }
}
