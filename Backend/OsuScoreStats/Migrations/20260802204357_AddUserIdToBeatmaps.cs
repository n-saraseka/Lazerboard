using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToBeatmaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "preview_url",
                table: "beatmapsets");

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "beatmaps",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_beatmaps_user_id",
                table: "beatmaps",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_beatmaps_users_user_id",
                table: "beatmaps",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_beatmaps_users_user_id",
                table: "beatmaps");

            migrationBuilder.DropIndex(
                name: "ix_beatmaps_user_id",
                table: "beatmaps");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "beatmaps");

            migrationBuilder.AddColumn<string>(
                name: "preview_url",
                table: "beatmapsets",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
