using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCollectionPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Collections",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "PathNames",
                table: "Collections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "57f37420-c260-4f18-93b3-3a8da05af1e4", new DateTime(2025, 5, 26, 23, 28, 22, 143, DateTimeKind.Utc).AddTicks(9524), "AQAAAAIAAYagAAAAEPcPA6QOGPjdZyJ5lA+JAAfB7mXtEc1ABwRjM9nIfAbNj/ZDXxtLeTYc0OGrpbueKQ==", "c4b06b2a-aa37-4e8f-98d0-c1f6853c1fff" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PathNames",
                table: "Collections");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Collections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a6cbf29f-54ac-4750-bcf2-3bf8ef26901e", new DateTime(2025, 5, 12, 21, 11, 50, 755, DateTimeKind.Utc).AddTicks(2919), "AQAAAAIAAYagAAAAENU+kshBx6fKWJphKpvTfgA47TzhTZ/fvpIlqDFK6KhyIheJYxBjTUi6y4/jCHRAPg==", "a3b27572-99a8-4d84-af2f-6d33ff695047" });
        }
    }
}
