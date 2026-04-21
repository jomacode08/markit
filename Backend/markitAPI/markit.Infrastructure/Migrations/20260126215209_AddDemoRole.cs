using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "security",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "3c2cba5a-562c-4098-9598-864e96f15397", null, "Demo", "demo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "security",
                table: "Roles",
                keyColumn: "Id",
                keyValue: "3c2cba5a-562c-4098-9598-864e96f15397");
        }
    }
}
