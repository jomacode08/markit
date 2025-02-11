using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderToBlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Blocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "ca6b1e85-db19-4700-b5a7-59f0691ef2ff",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "740018a6-2657-48e6-b7aa-60e4657cd084", new DateTime(2025, 2, 7, 23, 59, 7, 710, DateTimeKind.Utc).AddTicks(2804), "AQAAAAIAAYagAAAAEAHTc2k0zaSQVLgbuIrW0upINfwwPTT7WYStlIexZvEiSHDREF+Y5cxgDyO0rnhe1w==", "5b862f56-8505-4886-9912-8972a56c64a1" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Blocks");

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "ca6b1e85-db19-4700-b5a7-59f0691ef2ff",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d8be238f-3253-49f1-9a66-1345bb4ef4dc", new DateTime(2025, 2, 5, 22, 46, 56, 454, DateTimeKind.Utc).AddTicks(6213), "AQAAAAIAAYagAAAAEKZoEmPxWy2VbE6GwaFkSLVehfM0oIqn52ruIq4/3h+gkt3qjaLnSWrcrhE7wfIKXQ==", "534c0c69-53f2-475a-8bc4-85dd1189b1a6" });
        }
    }
}
