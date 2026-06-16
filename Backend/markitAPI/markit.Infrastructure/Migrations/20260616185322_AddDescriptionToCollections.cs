using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "collections",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "collections");
        }
    }
}
