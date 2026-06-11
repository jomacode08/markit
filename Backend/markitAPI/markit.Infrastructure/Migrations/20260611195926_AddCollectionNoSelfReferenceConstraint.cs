using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionNoSelfReferenceConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "ck_collections_no_self_reference",
                table: "collections",
                sql: "parent_id <> id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_collections_no_self_reference",
                table: "collections");
        }
    }
}
