using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmojiToCollectionAndMark : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Emoji",
                table: "Marks",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true,
                collation: "Latin1_General_100_CI_AS_SC");

            migrationBuilder.AddColumn<string>(
                name: "Emoji",
                table: "Collections",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true,
                collation: "Latin1_General_100_CI_AS_SC");

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ad32df2e-be4f-4cd6-a8bb-f47ae0865311", new DateTime(2025, 7, 24, 1, 24, 16, 993, DateTimeKind.Utc).AddTicks(5275), "AQAAAAIAAYagAAAAEAzuCuNWu1D0zxxB9pwCFMrD/aLh3tb33JIYhf7EaOX/vi2dLZJXGH/92mli1N0dTw==", "026e8196-fffc-45d0-9697-c5ffe1544ceb" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Emoji",
                table: "Marks");

            migrationBuilder.DropColumn(
                name: "Emoji",
                table: "Collections");

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7280f25f-1e7f-4d4e-b7d2-f0c3d64fb0ff", new DateTime(2025, 7, 2, 19, 43, 12, 3, DateTimeKind.Utc).AddTicks(2471), "AQAAAAIAAYagAAAAEL2SIK+badvOkYJFQ+NVqTSeOwEhFnN0naz78RmlwstRt4XkpYhaXHQbhFp0X67XOw==", "e04aa47e-6b83-4c10-88b6-e1df4cdc2fed" });
        }
    }
}
