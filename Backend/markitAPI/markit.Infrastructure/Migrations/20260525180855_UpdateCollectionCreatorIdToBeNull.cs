using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCollectionCreatorIdToBeNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_collections_creators_creator_id",
                table: "collections");

            migrationBuilder.AlterColumn<int>(
                name: "creator_id",
                table: "collections",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "fk_collections_creators_creator_id",
                table: "collections",
                column: "creator_id",
                principalTable: "creators",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_collections_creators_creator_id",
                table: "collections");

            migrationBuilder.AlterColumn<int>(
                name: "creator_id",
                table: "collections",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_collections_creators_creator_id",
                table: "collections",
                column: "creator_id",
                principalTable: "creators",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
