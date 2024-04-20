using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class FeeCharge2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FeeCharge_LeaseId",
                table: "FeeCharge",
                column: "LeaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_FeeCharge_Lease_LeaseId",
                table: "FeeCharge",
                column: "LeaseId",
                principalTable: "Lease",
                principalColumn: "LeaseId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeeCharge_Lease_LeaseId",
                table: "FeeCharge");

            migrationBuilder.DropIndex(
                name: "IX_FeeCharge_LeaseId",
                table: "FeeCharge");
        }
    }
}
