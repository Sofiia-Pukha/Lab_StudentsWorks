using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMVC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordHashAndRoleEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Додаємо колонку PasswordHash
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Конвертуємо Role з text в integer
            // Спочатку додаємо нову колонку
            migrationBuilder.AddColumn<int>(
                name: "RoleInt",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 1); // 1 = StudentAuthor

            // Копіюємо дані (Admin -> 0, решта -> 1)
            migrationBuilder.Sql(@"
                UPDATE ""Users"" SET ""RoleInt"" = 
                    CASE WHEN ""Role"" = 'Admin' THEN 0 ELSE 1 END
            ");

            // Видаляємо стару колонку
            migrationBuilder.DropColumn(name: "Role", table: "Users");

            // Перейменовуємо нову
            migrationBuilder.RenameColumn(name: "RoleInt", newName: "Role", table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoleStr",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "StudentAuthor");

            migrationBuilder.Sql(@"
                UPDATE ""Users"" SET ""RoleStr"" = 
                    CASE WHEN ""Role"" = 0 THEN 'Admin' ELSE 'StudentAuthor' END
            ");

            migrationBuilder.DropColumn(name: "Role", table: "Users");
            migrationBuilder.DropColumn(name: "PasswordHash", table: "Users");
            migrationBuilder.RenameColumn(name: "RoleStr", newName: "Role", table: "Users");
        }
    }
}
