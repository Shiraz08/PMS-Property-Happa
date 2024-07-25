using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class Added_StripeSubscription_Tbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentGuids",
                columns: table => new
                {
                    Guid = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_PaymentGuids", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "PaymentInformations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AmountCharged = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ChargeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_PaymentInformations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethodInformations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardHolderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardLast4Digit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryMonth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryYear = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GUID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentMethodId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_PaymentMethodInformations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StripeSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubscriptionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCanceled = table.Column<bool>(type: "bit", nullable: false),
                    BillingInterval = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubscriptionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsTrial = table.Column<bool>(type: "bit", nullable: true),
                    GUID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StripeSubscriptions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentGuids");

            migrationBuilder.DropTable(
                name: "PaymentInformations");

            migrationBuilder.DropTable(
                name: "PaymentMethodInformations");

            migrationBuilder.DropTable(
                name: "StripeSubscriptions");
        }
    }
}
