using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class added_Budget_Tbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Budgets",
                columns: table => new
                {
                    BudgetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BudgetType = table.Column<int>(type: "int", nullable: false),
                    BudgetBy = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    StartingMonth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FiscalYear = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Period = table.Column<int>(type: "int", nullable: false),
                    ReferenceData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountingMethod = table.Column<int>(type: "int", nullable: false),
                    ShowReferenceData = table.Column<bool>(type: "bit", nullable: false),
                    BudgetDuplicateId = table.Column<int>(type: "int", nullable: true),
                    DuplicatedBudgetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DuplicationDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsDuplicated = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Budgets", x => x.BudgetId);
                });

            migrationBuilder.CreateTable(
                name: "BudgetItem",
                columns: table => new
                {
                    BudgetItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Period = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BudgetId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_BudgetItem", x => x.BudgetItemId);
                    table.ForeignKey(
                        name: "FK_BudgetItem_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "Budgets",
                        principalColumn: "BudgetId");
                });

            migrationBuilder.CreateTable(
                name: "BudgetItemMonth",
                columns: table => new
                {
                    BudgetItemMonthID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetItemId = table.Column<int>(type: "int", nullable: false),
                    Jan = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Feb = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    March = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    April = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    May = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    June = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    July = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Aug = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Sep = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Oct = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Nov = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Dec = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    YearStart = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    YearEnd = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    quat1 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    quat2 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    quat4 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    quat5 = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetItemMonth", x => x.BudgetItemMonthID);
                    table.ForeignKey(
                        name: "FK_BudgetItemMonth_BudgetItem_BudgetItemId",
                        column: x => x.BudgetItemId,
                        principalTable: "BudgetItem",
                        principalColumn: "BudgetItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItem_BudgetId",
                table: "BudgetItem",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItemMonth_BudgetItemId",
                table: "BudgetItemMonth",
                column: "BudgetItemId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetItemMonth");

            migrationBuilder.DropTable(
                name: "BudgetItem");

            migrationBuilder.DropTable(
                name: "Budgets");
        }
    }
}
