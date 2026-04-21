using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Marks");

            migrationBuilder.CreateTable(
                name: "Blocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cols = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MarkId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Enable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blocks_Marks_MarkId",
                        column: x => x.MarkId,
                        principalTable: "Marks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "ca6b1e85-db19-4700-b5a7-59f0691ef2ff",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "404d2cf6-8ad5-45c8-9519-0f17186532e0", new DateTime(2025, 1, 31, 0, 20, 42, 268, DateTimeKind.Utc).AddTicks(6748), "AQAAAAIAAYagAAAAEL4/4Ko9HgqIsqSs469OZTDOivkwO1kRPiX/0w4P3Vtm8z2YoO1aIX5TdP5npYlI5A==", "274e3e6c-f26b-44dc-ae23-b0532feed55f" });

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_MarkId",
                table: "Blocks",
                column: "MarkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blocks");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Marks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "ca6b1e85-db19-4700-b5a7-59f0691ef2ff",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7abd8985-dc00-4a5c-bd1e-5528ada39c82", new DateTime(2024, 12, 18, 18, 59, 27, 569, DateTimeKind.Utc).AddTicks(8986), "AQAAAAIAAYagAAAAEEBjNlv7ITOB3X/7Jea0u3ySXOwBNFzZQYi/WTbvmsmh4DA7DGRPvd2PceLj/O1ZaQ==", "591ac5cd-f254-45dd-98e0-24e8ad97915b" });
        }
    }
}
