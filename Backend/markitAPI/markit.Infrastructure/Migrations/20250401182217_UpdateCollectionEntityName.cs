using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCollectionEntityName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collection_Collection_ParentId",
                table: "Collection");

            migrationBuilder.DropForeignKey(
                name: "FK_Collection_Creators_CreatorId",
                table: "Collection");

            migrationBuilder.DropForeignKey(
                name: "FK_Marks_Collection_CollectionId",
                table: "Marks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Collection",
                table: "Collection");

            migrationBuilder.RenameTable(
                name: "Collection",
                newName: "Collections");

            migrationBuilder.RenameIndex(
                name: "IX_Collection_ParentId",
                table: "Collections",
                newName: "IX_Collections_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_Collection_CreatorId",
                table: "Collections",
                newName: "IX_Collections_CreatorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Collections",
                table: "Collections",
                column: "Id");

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "463cacb8-c466-4dbe-8de4-6129fab09d9d", new DateTime(2025, 4, 1, 18, 22, 16, 446, DateTimeKind.Utc).AddTicks(930), "AQAAAAIAAYagAAAAEHyK/L8LWPLiH0gEuKlmz3v0cymqx2p/lfveEsfyEkaBUoRFpNIfJENRhUXH8Hgzhw==", "f67383fa-0941-44c8-a241-671fe7ce16b2" });

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_Collections_ParentId",
                table: "Collections",
                column: "ParentId",
                principalTable: "Collections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_Creators_CreatorId",
                table: "Collections",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Marks_Collections_CollectionId",
                table: "Marks",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collections_Collections_ParentId",
                table: "Collections");

            migrationBuilder.DropForeignKey(
                name: "FK_Collections_Creators_CreatorId",
                table: "Collections");

            migrationBuilder.DropForeignKey(
                name: "FK_Marks_Collections_CollectionId",
                table: "Marks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Collections",
                table: "Collections");

            migrationBuilder.RenameTable(
                name: "Collections",
                newName: "Collection");

            migrationBuilder.RenameIndex(
                name: "IX_Collections_ParentId",
                table: "Collection",
                newName: "IX_Collection_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_Collections_CreatorId",
                table: "Collection",
                newName: "IX_Collection_CreatorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Collection",
                table: "Collection",
                column: "Id");

            migrationBuilder.UpdateData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5dcb4166-fe16-4afe-947c-532ba5559ea2", new DateTime(2025, 3, 27, 18, 9, 22, 770, DateTimeKind.Utc).AddTicks(8907), "AQAAAAIAAYagAAAAENrgfnOZbmKS0+MrCaNpI72NF/dPygwI6NZhVWp7V/0m4w7EbdVPVcBOz/dl1eC+8A==", "9bfa337d-8beb-4ae5-a839-1f0075987fd6" });

            migrationBuilder.AddForeignKey(
                name: "FK_Collection_Collection_ParentId",
                table: "Collection",
                column: "ParentId",
                principalTable: "Collection",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Collection_Creators_CreatorId",
                table: "Collection",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Marks_Collection_CollectionId",
                table: "Marks",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
