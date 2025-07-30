using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMaxLengthToEmojiColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Emoji",
                table: "Marks",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Latin1_General_100_CI_AS_SC",
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2,
                oldNullable: true,
                oldCollation: "Latin1_General_100_CI_AS_SC");

            migrationBuilder.AlterColumn<string>(
                name: "Emoji",
                table: "Collections",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Latin1_General_100_CI_AS_SC",
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2,
                oldNullable: true,
                oldCollation: "Latin1_General_100_CI_AS_SC");

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e1e19640-0d5a-4b4c-854b-ec68f19a4fb3", new DateTime(2025, 7, 30, 19, 48, 22, 732, DateTimeKind.Utc).AddTicks(5229), "AQAAAAIAAYagAAAAEAKH+z+lvSd92Afv4fLHQJh54oZkb0M3gbkbawDGDX9VdwPbaxzpb7rrLUHIKF6y1w==", "47ed6e46-007a-490f-8dba-91ce7909d2da" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Emoji",
                table: "Marks",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true,
                collation: "Latin1_General_100_CI_AS_SC",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldCollation: "Latin1_General_100_CI_AS_SC");

            migrationBuilder.AlterColumn<string>(
                name: "Emoji",
                table: "Collections",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true,
                collation: "Latin1_General_100_CI_AS_SC",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldCollation: "Latin1_General_100_CI_AS_SC");

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ad32df2e-be4f-4cd6-a8bb-f47ae0865311", new DateTime(2025, 7, 24, 1, 24, 16, 993, DateTimeKind.Utc).AddTicks(5275), "AQAAAAIAAYagAAAAEAzuCuNWu1D0zxxB9pwCFMrD/aLh3tb33JIYhf7EaOX/vi2dLZJXGH/92mli1N0dTw==", "026e8196-fffc-45d0-9697-c5ffe1544ceb" });
        }
    }
}
