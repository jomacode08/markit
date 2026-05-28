using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameDemoRoleToGuest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "security",
                table: "roles",
                keyColumn: "id",
                keyValue: "3c2cba5a-562c-4098-9598-864e96f15397",
                columns: new[] { "name", "normalized_name" },
                values: new object[] { "Guest", "GUEST" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "security",
                table: "roles",
                keyColumn: "id",
                keyValue: "3c2cba5a-562c-4098-9598-864e96f15397",
                columns: new[] { "name", "normalized_name" },
                values: new object[] { "Demo", "DEMO" });
        }
    }
}
