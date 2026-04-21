using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNameLessToMark : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NameLess",
                table: "Marks",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameLess",
                table: "Marks");
        }
    }
}
