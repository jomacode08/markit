using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCollectionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "document_id",
                table: "collections");

            migrationBuilder.DropColumn(
                name: "last_sync",
                table: "collections");

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "collections",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_collections_user_id",
                table: "collections",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_collections_asp_net_users_user_id",
                table: "collections",
                column: "user_id",
                principalSchema: "security",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_collections_asp_net_users_user_id",
                table: "collections");

            migrationBuilder.DropIndex(
                name: "ix_collections_user_id",
                table: "collections");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "collections");

            migrationBuilder.AddColumn<string>(
                name: "document_id",
                table: "collections",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_sync",
                table: "collections",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
