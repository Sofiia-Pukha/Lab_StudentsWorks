using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMVC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminLogStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDelete",
                table: "AdminLogs");

            migrationBuilder.RenameColumn(
                name: "ActionType",
                table: "AdminLogs",
                newName: "TargetColumn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TargetColumn",
                table: "AdminLogs",
                newName: "ActionType");

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                table: "AdminLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
