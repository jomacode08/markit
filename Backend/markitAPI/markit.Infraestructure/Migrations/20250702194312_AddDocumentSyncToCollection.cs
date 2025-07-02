using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentSyncToCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentId",
                table: "Collections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSync",
                table: "Collections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7280f25f-1e7f-4d4e-b7d2-f0c3d64fb0ff", new DateTime(2025, 7, 2, 19, 43, 12, 3, DateTimeKind.Utc).AddTicks(2471), "AQAAAAIAAYagAAAAEL2SIK+badvOkYJFQ+NVqTSeOwEhFnN0naz78RmlwstRt4XkpYhaXHQbhFp0X67XOw==", "e04aa47e-6b83-4c10-88b6-e1df4cdc2fed" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "LastSync",
                table: "Collections");

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "351e226c-dc5b-413c-9141-1762c307f098", new DateTime(2025, 7, 2, 19, 42, 40, 707, DateTimeKind.Utc).AddTicks(9011), "AQAAAAIAAYagAAAAECOxHGWndHL8/EHqeruOA4wtr16hiW5QO2sYS6Ej2NTEYF2S7ixf/ldP8APTtIFe5w==", "a0ed06f1-a9f6-482d-acb5-1b9efcabede5" });
        }
    }
}
