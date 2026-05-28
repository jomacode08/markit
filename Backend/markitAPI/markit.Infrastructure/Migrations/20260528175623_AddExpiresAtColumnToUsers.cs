using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpiresAtColumnToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "expires_at",
                schema: "security",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_expires_at",
                schema: "security",
                table: "users",
                column: "expires_at",
                filter: "expires_at IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_expires_at",
                schema: "security",
                table: "users");

            migrationBuilder.DropColumn(
                name: "expires_at",
                schema: "security",
                table: "users");
        }
    }
}
