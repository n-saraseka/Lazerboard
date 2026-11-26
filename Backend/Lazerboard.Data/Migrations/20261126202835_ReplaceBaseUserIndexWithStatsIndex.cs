using Lazerboard.Data.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceBaseUserIndexWithStatsIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_beatmap_id",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_user_id",
                table: "scores");

            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS ix_scores_user_id_stats ON scores(user_id) INCLUDE (date, rank, speed_change, beatmap_id)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_beatmap_id",
                table: "scores",
                column: "beatmap_id");

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_user_id",
                table: "scores",
                column: "user_id");
        }
    }
}
