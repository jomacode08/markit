using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace markit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_collections_creators_creator_id",
                table: "collections");

            migrationBuilder.DropTable(
                name: "creators");

            migrationBuilder.DropIndex(
                name: "ix_collections_creator_id",
                table: "collections");

            migrationBuilder.DropColumn(
                name: "creator_id",
                schema: "security",
                table: "users");

            migrationBuilder.DropColumn(
                name: "creator_id",
                table: "collections");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "creator_id",
                schema: "security",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "creator_id",
                table: "collections",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "creators",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    enable = table.Column<bool>(type: "boolean", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    gender = table.Column<int>(type: "integer", nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_creators", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_collections_creator_id",
                table: "collections",
                column: "creator_id");

            migrationBuilder.AddForeignKey(
                name: "fk_collections_creators_creator_id",
                table: "collections",
                column: "creator_id",
                principalTable: "creators",
                principalColumn: "id");
        }
    }
}
