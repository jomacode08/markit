using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDocumentModelFromMarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "document_id",
                table: "marks");

            migrationBuilder.DropColumn(
                name: "last_sync",
                table: "marks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "document_id",
                table: "marks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_sync",
                table: "marks",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
