using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorFkToMark : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "ca6b1e85-db19-4700-b5a7-59f0691ef2ff",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7abd8985-dc00-4a5c-bd1e-5528ada39c82", new DateTime(2024, 12, 18, 18, 59, 27, 569, DateTimeKind.Utc).AddTicks(8986), "AQAAAAIAAYagAAAAEEBjNlv7ITOB3X/7Jea0u3ySXOwBNFzZQYi/WTbvmsmh4DA7DGRPvd2PceLj/O1ZaQ==", "591ac5cd-f254-45dd-98e0-24e8ad97915b" });

            migrationBuilder.CreateIndex(
                name: "IX_Marks_CreatorId",
                table: "Marks",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Marks_Creators_CreatorId",
                table: "Marks",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Marks_Creators_CreatorId",
                table: "Marks");

            migrationBuilder.DropIndex(
                name: "IX_Marks_CreatorId",
                table: "Marks");

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "ca6b1e85-db19-4700-b5a7-59f0691ef2ff",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6785a543-8581-430b-8b4e-639180b0cbfc", new DateTime(2024, 12, 18, 18, 54, 15, 250, DateTimeKind.Utc).AddTicks(7278), "AQAAAAIAAYagAAAAEG+S27KyNznPP81SF4vq4Jr7YykbPYw+mab77v8XB/jRN1khF0p2G/8S6v2ug4agdg==", "4fd32c9b-f1ed-476b-8eb4-f9a44682a555" });
        }
    }
}
