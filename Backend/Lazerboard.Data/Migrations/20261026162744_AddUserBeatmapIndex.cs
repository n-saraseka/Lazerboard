using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBeatmapIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_rank_mode",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_beatmap_id",
                table: "scores",
                columns: new[] { "user_id", "beatmap_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_beatmap_id",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_rank_mode",
                table: "scores",
                columns: new[] { "rank", "mode" });
        }
    }
}
