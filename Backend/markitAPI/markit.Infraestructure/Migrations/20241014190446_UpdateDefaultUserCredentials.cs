using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDefaultUserCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "ca6b1e85-db19-4700-b5a7-59f0691ef2ff",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "Email", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "SecurityStamp", "UserName" },
                values: new object[] { "e94b8073-0fd1-4951-b0ce-18d66582e921", new DateTime(2024, 10, 14, 19, 4, 46, 366, DateTimeKind.Utc).AddTicks(6179), "jomacode8@gmail.com", "JOMACODE8@GMAIL.COM", "JOMACODE8@GMAIL.COM", "AQAAAAIAAYagAAAAEA0/ag+Y22YuaAO7yygFPUqpONGSolgGx9BiYwlCxwebC7YRuIxWi/23ehkvlg2mOw==", "d4728ead-3fe1-4ea3-ba9b-7992252ba3ec", "jomacode8@gmail.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "ca6b1e85-db19-4700-b5a7-59f0691ef2ff",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "Email", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "SecurityStamp", "UserName" },
                values: new object[] { "bae06e2f-37e4-4e35-9d6b-527a1cf44e4d", new DateTime(2024, 10, 1, 20, 19, 8, 620, DateTimeKind.Utc).AddTicks(635), "jomacode.me8@gmail.com", null, null, "AQAAAAIAAYagAAAAEE5tnlTHgaGUh8SgHeTUBrOGLGBs9h+cd7dgCog/W42jgFDm+TCbKVc3bfv4sl/XQQ==", "fad8ac2e-eea5-43b6-b007-21f563b3a0ea", "jomacode.me8@gmail.com" });
        }
    }
}
