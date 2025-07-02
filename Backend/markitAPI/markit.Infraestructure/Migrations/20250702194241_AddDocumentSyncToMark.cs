using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentSyncToMark : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentId",
                table: "Marks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSync",
                table: "Marks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "351e226c-dc5b-413c-9141-1762c307f098", new DateTime(2025, 7, 2, 19, 42, 40, 707, DateTimeKind.Utc).AddTicks(9011), "AQAAAAIAAYagAAAAECOxHGWndHL8/EHqeruOA4wtr16hiW5QO2sYS6Ej2NTEYF2S7ixf/ldP8APTtIFe5w==", "a0ed06f1-a9f6-482d-acb5-1b9efcabede5" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Marks");

            migrationBuilder.DropColumn(
                name: "LastSync",
                table: "Marks");

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ad3f6c7a-4ea3-49a7-9984-c847edef6469", new DateTime(2025, 6, 12, 20, 9, 23, 660, DateTimeKind.Utc).AddTicks(4095), "AQAAAAIAAYagAAAAEPiLyszTa13zL3Mys4Px5jttwWnmsvbjeLW20UpWi8CcJMr5y2ovrFw+bfTJz4JcRw==", "cba6d0db-96f9-46f9-91fe-6b72c4deabe7" });
        }
    }
}
