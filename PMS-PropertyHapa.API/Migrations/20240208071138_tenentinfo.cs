using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.API.Migrations
{
    public partial class tenentinfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantOrganizationInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationIcon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationLogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizatioPrimaryColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationSecondColor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantOrganizationInfo", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantOrganizationInfo");
        }
    }
}
