using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace markit.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultSystemConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "security",
                table: "SystemConfigs",
                columns: new[] { "Id", "Description", "Value" },
                values: new object[] { "Demo:IsEnabled", "Configuration that toggles the demo features of the app.", "false" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "security",
                table: "SystemConfigs",
                keyColumn: "Id",
                keyValue: "Demo:IsEnabled");
        }
    }
}
