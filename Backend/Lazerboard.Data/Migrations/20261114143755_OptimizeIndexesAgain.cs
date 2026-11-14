using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeIndexesAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_is_convert_mode_date",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_beatmap_id",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_is_convert_mode_date",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_mode_rank",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_speed_change",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_beatmapsets_finished_scanning_at",
                table: "beatmapsets");

            migrationBuilder.DropIndex(
                name: "ix_beatmapsets_main_started_processing_at",
                table: "beatmapsets");

            migrationBuilder.DropIndex(
                name: "ix_beatmapsets_secondary_finished_processing_at",
                table: "beatmapsets");

            migrationBuilder.DropIndex(
                name: "ix_beatmapsets_secondary_started_processing_at",
                table: "beatmapsets");

            migrationBuilder.DropIndex(
                name: "ix_beatmapsets_started_scanning_at",
                table: "beatmapsets");

            migrationBuilder.CreateIndex(
                name: "ix_scores_date_mode",
                table: "scores",
                columns: new[] { "date", "mode" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_beatmap_id_mode",
                table: "scores",
                columns: new[] { "user_id", "beatmap_id", "mode" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_date",
                table: "scores",
                columns: new[] { "user_id", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_rank_mode",
                table: "scores",
                columns: new[] { "user_id", "rank", "mode" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_speed_change_mode",
                table: "scores",
                columns: new[] { "user_id", "speed_change", "mode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_date_mode",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_beatmap_id_mode",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_date",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_rank_mode",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_speed_change_mode",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_is_convert_mode_date",
                table: "scores",
                columns: new[] { "is_convert", "mode", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_beatmap_id",
                table: "scores",
                columns: new[] { "user_id", "beatmap_id" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_is_convert_mode_date",
                table: "scores",
                columns: new[] { "user_id", "is_convert", "mode", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_mode_rank",
                table: "scores",
                columns: new[] { "user_id", "mode", "rank" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_speed_change",
                table: "scores",
                columns: new[] { "user_id", "speed_change" });

            migrationBuilder.CreateIndex(
                name: "ix_beatmapsets_finished_scanning_at",
                table: "beatmapsets",
                column: "finished_scanning_at");

            migrationBuilder.CreateIndex(
                name: "ix_beatmapsets_main_started_processing_at",
                table: "beatmapsets",
                column: "main_started_processing_at");

            migrationBuilder.CreateIndex(
                name: "ix_beatmapsets_secondary_finished_processing_at",
                table: "beatmapsets",
                column: "secondary_finished_processing_at");

            migrationBuilder.CreateIndex(
                name: "ix_beatmapsets_secondary_started_processing_at",
                table: "beatmapsets",
                column: "secondary_started_processing_at");

            migrationBuilder.CreateIndex(
                name: "ix_beatmapsets_started_scanning_at",
                table: "beatmapsets",
                column: "started_scanning_at");
        }
    }
}
