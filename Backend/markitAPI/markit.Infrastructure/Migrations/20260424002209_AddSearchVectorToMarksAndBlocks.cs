using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace markit.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchVectorToMarksAndBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "search_vector",
                table: "marks",
                type: "tsvector",
                nullable: true)
                .Annotation("Npgsql:TsVectorConfig", "english")
                .Annotation("Npgsql:TsVectorProperties", new[] { "name" });

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "search_vector",
                table: "blocks",
                type: "tsvector",
                nullable: true)
                .Annotation("Npgsql:TsVectorConfig", "english")
                .Annotation("Npgsql:TsVectorProperties", new[] { "title", "content" });

            migrationBuilder.CreateIndex(
                name: "ix_marks_search_vector",
                table: "marks",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_blocks_search_vector",
                table: "blocks",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_marks_search_vector",
                table: "marks");

            migrationBuilder.DropIndex(
                name: "ix_blocks_search_vector",
                table: "blocks");

            migrationBuilder.DropColumn(
                name: "search_vector",
                table: "marks");

            migrationBuilder.DropColumn(
                name: "search_vector",
                table: "blocks");
        }
    }
}
