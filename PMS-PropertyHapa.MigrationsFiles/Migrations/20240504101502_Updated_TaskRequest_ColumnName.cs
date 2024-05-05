using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS_PropertyHapa.MigrationsFiles.Migrations
{
    public partial class Updated_TaskRequest_ColumnName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOneTimeWorkOrder",
                table: "TaskRequest");

            migrationBuilder.DropColumn(
                name: "IsRecurringWorkOrder",
                table: "TaskRequest");

            migrationBuilder.RenameColumn(
                name: "TaksRequestFile",
                table: "TaskRequest",
                newName: "TaskRequestFile");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaskRequestFile",
                table: "TaskRequest",
                newName: "TaksRequestFile");

            migrationBuilder.AddColumn<bool>(
                name: "IsOneTimeWorkOrder",
                table: "TaskRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurringWorkOrder",
                table: "TaskRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
