using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropUnusedBlockColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "Cols",
                table: "Blocks");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Blocks",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a6cbf29f-54ac-4750-bcf2-3bf8ef26901e", new DateTime(2025, 5, 12, 21, 11, 50, 755, DateTimeKind.Utc).AddTicks(2919), "AQAAAAIAAYagAAAAENU+kshBx6fKWJphKpvTfgA47TzhTZ/fvpIlqDFK6KhyIheJYxBjTUi6y4/jCHRAPg==", "a3b27572-99a8-4d84-af2f-6d33ff695047" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Blocks",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<int>(
                name: "Color",
                table: "Blocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Cols",
                table: "Blocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "463cacb8-c466-4dbe-8de4-6129fab09d9d", new DateTime(2025, 4, 1, 18, 22, 16, 446, DateTimeKind.Utc).AddTicks(930), "AQAAAAIAAYagAAAAEHyK/L8LWPLiH0gEuKlmz3v0cymqx2p/lfveEsfyEkaBUoRFpNIfJENRhUXH8Hgzhw==", "f67383fa-0941-44c8-a241-671fe7ce16b2" });
        }
    }
}
