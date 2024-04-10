using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class Happa1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Communication",
                columns: table => new
                {
                    Communication_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PropertyIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsByEmail = table.Column<bool>(type: "bit", nullable: false),
                    IsByText = table.Column<bool>(type: "bit", nullable: false),
                    IsShowCommunicationInTenantPortal = table.Column<bool>(type: "bit", nullable: false),
                    IsPostOnTenantScreen = table.Column<bool>(type: "bit", nullable: false),
                    RemoveFeedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Communication_File = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Communication", x => x.Communication_Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Communication");
        }
    }
}
