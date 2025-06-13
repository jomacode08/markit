using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class addIsFavoriteToCollectionAndMark : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "Marks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "Collections",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ad3f6c7a-4ea3-49a7-9984-c847edef6469", new DateTime(2025, 6, 12, 20, 9, 23, 660, DateTimeKind.Utc).AddTicks(4095), "AQAAAAIAAYagAAAAEPiLyszTa13zL3Mys4Px5jttwWnmsvbjeLW20UpWi8CcJMr5y2ovrFw+bfTJz4JcRw==", "cba6d0db-96f9-46f9-91fe-6b72c4deabe7" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "Marks");

            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "Collections");

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "57f37420-c260-4f18-93b3-3a8da05af1e4", new DateTime(2025, 5, 26, 23, 28, 22, 143, DateTimeKind.Utc).AddTicks(9524), "AQAAAAIAAYagAAAAEPcPA6QOGPjdZyJ5lA+JAAfB7mXtEc1ABwRjM9nIfAbNj/ZDXxtLeTYc0OGrpbueKQ==", "c4b06b2a-aa37-4e8f-98d0-c1f6853c1fff" });
        }
    }
}
