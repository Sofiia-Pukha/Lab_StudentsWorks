using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMVC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeDatabaseStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FileUrl",
                table: "Works",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "TeacherId1",
                table: "Works",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId1",
                table: "Teachers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Works_TeacherId1",
                table: "Works",
                column: "TeacherId1");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_DepartmentId1",
                table: "Teachers",
                column: "DepartmentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Departments_DepartmentId1",
                table: "Teachers",
                column: "DepartmentId1",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Works_Teachers_TeacherId1",
                table: "Works",
                column: "TeacherId1",
                principalTable: "Teachers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Departments_DepartmentId1",
                table: "Teachers");

            migrationBuilder.DropForeignKey(
                name: "FK_Works_Teachers_TeacherId1",
                table: "Works");

            migrationBuilder.DropIndex(
                name: "IX_Works_TeacherId1",
                table: "Works");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_DepartmentId1",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "TeacherId1",
                table: "Works");

            migrationBuilder.DropColumn(
                name: "DepartmentId1",
                table: "Teachers");

            migrationBuilder.AlterColumn<string>(
                name: "FileUrl",
                table: "Works",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
