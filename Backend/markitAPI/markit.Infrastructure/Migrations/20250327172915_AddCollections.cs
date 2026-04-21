using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "security",
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "5335f3ce-37dd-11ee-be56-0242ac120002", "ca6b1e85-db19-4700-b5a7-59f0691ef2ff" });

            migrationBuilder.DeleteData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "ca6b1e85-db19-4700-b5a7-59f0691ef2ff");

            migrationBuilder.AddColumn<int>(
                name: "CollectionId",
                table: "Marks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Collection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    CreatorId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Enable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Collection_Collection_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Collection",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Collection_Creators_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Creators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "security",
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "AccessType", "ConcurrencyStamp", "CreatedDate", "CreatorId", "Email", "EmailConfirmed", "GivenName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Picture", "RegistrationConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "89063556-7135-4382-a532-66f364e26b85", 0, 1, "04a7a91d-3bde-4827-9f8d-1ac6a68b9f20", new DateTime(2025, 3, 27, 17, 29, 14, 884, DateTimeKind.Utc).AddTicks(1358), null, "jomacode8@gmail.com", true, "Markit Admin", false, null, "JOMACODE8@GMAIL.COM", "JOMACODE8@GMAIL.COM", "AQAAAAIAAYagAAAAEIlo92gvm280xOOW6zbzI0SpveOaZwFPtUxo1J00nOPq3r6ZBJLHEm1GZeCjo4Nflw==", "0000000000", true, null, false, "6fa004c4-8432-4a7c-98e9-e6e51e5bc9fa", false, "jomacode8@gmail.com" });

            migrationBuilder.InsertData(
                schema: "security",
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "5335f3ce-37dd-11ee-be56-0242ac120002", "89063556-7135-4382-a532-66f364e26b85" });

            migrationBuilder.CreateIndex(
                name: "IX_Marks_CollectionId",
                table: "Marks",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Collection_CreatorId",
                table: "Collection",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Collection_ParentId",
                table: "Collection",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Marks_Collection_CollectionId",
                table: "Marks",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Marks_Collection_CollectionId",
                table: "Marks");

            migrationBuilder.DropTable(
                name: "Collection");

            migrationBuilder.DropIndex(
                name: "IX_Marks_CollectionId",
                table: "Marks");

            migrationBuilder.DeleteData(
                schema: "security",
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "5335f3ce-37dd-11ee-be56-0242ac120002", "89063556-7135-4382-a532-66f364e26b85" });

            migrationBuilder.DeleteData(
                schema: "security",
                table: "Users",
                keyColumn: "Id",
                keyValue: "89063556-7135-4382-a532-66f364e26b85");

            migrationBuilder.DropColumn(
                name: "CollectionId",
                table: "Marks");

            migrationBuilder.InsertData(
                schema: "security",
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "AccessType", "ConcurrencyStamp", "CreatedDate", "CreatorId", "Email", "EmailConfirmed", "GivenName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Picture", "RegistrationConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "ca6b1e85-db19-4700-b5a7-59f0691ef2ff", 0, 1, "740018a6-2657-48e6-b7aa-60e4657cd084", new DateTime(2025, 2, 7, 23, 59, 7, 710, DateTimeKind.Utc).AddTicks(2804), null, "jomacode8@gmail.com", true, "Markit Admin", false, null, "JOMACODE8@GMAIL.COM", "JOMACODE8@GMAIL.COM", "AQAAAAIAAYagAAAAEAHTc2k0zaSQVLgbuIrW0upINfwwPTT7WYStlIexZvEiSHDREF+Y5cxgDyO0rnhe1w==", "0000000000", true, null, false, "5b862f56-8505-4886-9912-8972a56c64a1", false, "jomacode8@gmail.com" });

            migrationBuilder.InsertData(
                schema: "security",
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "5335f3ce-37dd-11ee-be56-0242ac120002", "ca6b1e85-db19-4700-b5a7-59f0691ef2ff" });
        }
    }
}
