using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class added_vendorOrgnization_Tbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentAmount",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "Vendor");

            migrationBuilder.RenameColumn(
                name: "PropertyIds",
                table: "Vendor",
                newName: "UnitIds");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "Vendor",
                newName: "AccountSwift");

            migrationBuilder.AddColumn<string>(
                name: "AccountBank",
                table: "Vendor",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountCurrency",
                table: "Vendor",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountHolder",
                table: "Vendor",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountIBAN",
                table: "Vendor",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountName",
                table: "Vendor",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PropertyId",
                table: "Vendor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                table: "Owner",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VendorOrganization",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    OrganizationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationIcon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationLogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorOrganization", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VendorOrganization");

            migrationBuilder.DropColumn(
                name: "AccountBank",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "AccountCurrency",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "AccountHolder",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "AccountIBAN",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "AccountName",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "PropertyId",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "Owner");

            migrationBuilder.RenameColumn(
                name: "UnitIds",
                table: "Vendor",
                newName: "PropertyIds");

            migrationBuilder.RenameColumn(
                name: "AccountSwift",
                table: "Vendor",
                newName: "PaymentMethod");

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentAmount",
                table: "Vendor",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "Vendor",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
