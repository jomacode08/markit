using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleToBlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Blocks",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "ca6b1e85-db19-4700-b5a7-59f0691ef2ff",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d8be238f-3253-49f1-9a66-1345bb4ef4dc", new DateTime(2025, 2, 5, 22, 46, 56, 454, DateTimeKind.Utc).AddTicks(6213), "AQAAAAIAAYagAAAAEKZoEmPxWy2VbE6GwaFkSLVehfM0oIqn52ruIq4/3h+gkt3qjaLnSWrcrhE7wfIKXQ==", "534c0c69-53f2-475a-8bc4-85dd1189b1a6" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Blocks");

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "ca6b1e85-db19-4700-b5a7-59f0691ef2ff",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "404d2cf6-8ad5-45c8-9519-0f17186532e0", new DateTime(2025, 1, 31, 0, 20, 42, 268, DateTimeKind.Utc).AddTicks(6748), "AQAAAAIAAYagAAAAEL4/4Ko9HgqIsqSs469OZTDOivkwO1kRPiX/0w4P3Vtm8z2YoO1aIX5TdP5npYlI5A==", "274e3e6c-f26b-44dc-ae23-b0532feed55f" });
        }
    }
}
