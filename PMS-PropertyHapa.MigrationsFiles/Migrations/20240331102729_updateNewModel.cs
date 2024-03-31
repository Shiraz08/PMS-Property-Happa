using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class updateNewModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lease",
                columns: table => new
                {
                    LeaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSigned = table.Column<bool>(type: "bit", nullable: false),
                    SignatureImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsFixedTerm = table.Column<bool>(type: "bit", nullable: false),
                    IsMonthToMonth = table.Column<bool>(type: "bit", nullable: false),
                    HasSecurityDeposit = table.Column<bool>(type: "bit", nullable: false),
                    LateFeesPolicy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantsTenantId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_Lease", x => x.LeaseId);
                    table.ForeignKey(
                        name: "FK_Lease_Tenant_TenantsTenantId",
                        column: x => x.TenantsTenantId,
                        principalTable: "Tenant",
                        principalColumn: "TenantId");
                });

            migrationBuilder.CreateTable(
                name: "RentCharge",
                columns: table => new
                {
                    RentChargeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeaseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentCharge", x => x.RentChargeId);
                    table.ForeignKey(
                        name: "FK_RentCharge_Lease_LeaseId",
                        column: x => x.LeaseId,
                        principalTable: "Lease",
                        principalColumn: "LeaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityDeposit",
                columns: table => new
                {
                    SecurityDepositId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeaseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityDeposit", x => x.SecurityDepositId);
                    table.ForeignKey(
                        name: "FK_SecurityDeposit_Lease_LeaseId",
                        column: x => x.LeaseId,
                        principalTable: "Lease",
                        principalColumn: "LeaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lease_TenantsTenantId",
                table: "Lease",
                column: "TenantsTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RentCharge_LeaseId",
                table: "RentCharge",
                column: "LeaseId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityDeposit_LeaseId",
                table: "SecurityDeposit",
                column: "LeaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentCharge");

            migrationBuilder.DropTable(
                name: "SecurityDeposit");

            migrationBuilder.DropTable(
                name: "Lease");
        }
    }
}
