using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteCreatorFromMarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Marks_Collection_CollectionId",
                table: "Marks");

            migrationBuilder.DropForeignKey(
                name: "FK_Marks_Creators_CreatorId",
                table: "Marks");

            migrationBuilder.DropIndex(
                name: "IX_Marks_CreatorId",
                table: "Marks");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Marks");

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5dcb4166-fe16-4afe-947c-532ba5559ea2", new DateTime(2025, 3, 27, 18, 9, 22, 770, DateTimeKind.Utc).AddTicks(8907), "AQAAAAIAAYagAAAAENrgfnOZbmKS0+MrCaNpI72NF/dPygwI6NZhVWp7V/0m4w7EbdVPVcBOz/dl1eC+8A==", "9bfa337d-8beb-4ae5-a839-1f0075987fd6" });

            migrationBuilder.AddForeignKey(
                name: "FK_Marks_Collection_CollectionId",
                table: "Marks",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Marks_Collection_CollectionId",
                table: "Marks");

            migrationBuilder.AddColumn<int>(
                name: "CreatorId",
                table: "Marks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "04a7a91d-3bde-4827-9f8d-1ac6a68b9f20", new DateTime(2025, 3, 27, 17, 29, 14, 884, DateTimeKind.Utc).AddTicks(1358), "AQAAAAIAAYagAAAAEIlo92gvm280xOOW6zbzI0SpveOaZwFPtUxo1J00nOPq3r6ZBJLHEm1GZeCjo4Nflw==", "6fa004c4-8432-4a7c-98e9-e6e51e5bc9fa" });

            migrationBuilder.CreateIndex(
                name: "IX_Marks_CreatorId",
                table: "Marks",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Marks_Collection_CollectionId",
                table: "Marks",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Marks_Creators_CreatorId",
                table: "Marks",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
