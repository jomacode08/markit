using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "security",
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "AccessType", "ConcurrencyStamp", "CreatedDate", "CreatorId", "Email", "EmailConfirmed", "GivenName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Picture", "RegistrationConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "89063556-7135-4382-a532-66f364e26b85", 0, 1, "e1e19640-0d5a-4b4c-854b-ec68f19a4fb3", new DateTime(2025, 7, 30, 19, 48, 22, 732, DateTimeKind.Utc).AddTicks(5229), null, "jomacode8@gmail.com", true, "Markit Admin", false, null, "JOMACODE8@GMAIL.COM", "JOMACODE8@GMAIL.COM", "AQAAAAIAAYagAAAAEAKH+z+lvSd92Afv4fLHQJh54oZkb0M3gbkbawDGDX9VdwPbaxzpb7rrLUHIKF6y1w==", "0000000000", true, null, false, "47ed6e46-007a-490f-8dba-91ce7909d2da", false, "jomacode8@gmail.com" });

            migrationBuilder.InsertData(
                schema: "security",
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "5335f3ce-37dd-11ee-be56-0242ac120002", "89063556-7135-4382-a532-66f364e26b85" });
        }
    }
}
