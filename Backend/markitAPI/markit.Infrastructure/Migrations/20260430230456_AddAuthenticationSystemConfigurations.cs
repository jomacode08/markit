using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace markit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationSystemConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "security",
                table: "system_configs",
                columns: new[] { "id", "description", "value" },
                values: new object[,]
                {
                    { "Auth:IsGitHubEnabled", "Configuration that enables or disables GitHub authentication for the app.", "false" },
                    { "Auth:IsGoogleEnabled", "Configuration that enables or disables Google authentication for the app.", "false" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "security",
                table: "system_configs",
                keyColumn: "id",
                keyValue: "Auth:IsGitHubEnabled");

            migrationBuilder.DeleteData(
                schema: "security",
                table: "system_configs",
                keyColumn: "id",
                keyValue: "Auth:IsGoogleEnabled");
        }
    }
}
