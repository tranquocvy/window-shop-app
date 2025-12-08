using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechHaven.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppSettingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_app_settings_key",
                table: "app_settings");

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "app_settings",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user_id",
                table: "app_settings");

            migrationBuilder.CreateIndex(
                name: "ix_app_settings_key",
                table: "app_settings",
                column: "key");
        }
    }
}
