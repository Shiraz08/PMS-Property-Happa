using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class Added_Owner_Column : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NoOfUnits",
                table: "StripeSubscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoOfUnits",
                table: "PaymentInformations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "Owner",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Longitude",
                table: "Owner",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoOfUnits",
                table: "StripeSubscriptions");

            migrationBuilder.DropColumn(
                name: "NoOfUnits",
                table: "PaymentInformations");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Owner");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Owner");
        }
    }
}
