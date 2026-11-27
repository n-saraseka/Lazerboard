using Lazerboard.Data.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class IncludeConvertFlagInIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_accuracy_user_id_mode",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_beatmap_id_user_id_mode",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_date_mode",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_mode_user_id",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_pp_user_id_mode",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_rank_user_id_mode",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_speed_change_user_id_mode",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_user_id_date_mode",
                table: "scores");
            
            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_user_id_stats",
                table: "scores");

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_accuracy_user_id_mode_is_convert",
                table: "scores",
                columns: new[] { "accuracy", "user_id", "mode", "is_convert" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_beatmap_id_user_id_mode_is_convert",
                table: "scores",
                columns: new[] { "beatmap_id", "user_id", "mode", "is_convert" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_date_mode_is_convert",
                table: "scores",
                columns: new[] { "date", "mode", "is_convert" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_mode_user_id_is_convert",
                table: "scores",
                columns: new[] { "mode", "user_id", "is_convert" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_pp_user_id_mode_is_convert",
                table: "scores",
                columns: new[] { "pp", "user_id", "mode", "is_convert" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_rank_user_id_mode_is_convert",
                table: "scores",
                columns: new[] { "rank", "user_id", "mode", "is_convert" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_speed_change_user_id_mode_is_convert",
                table: "scores",
                columns: new[] { "speed_change", "user_id", "mode", "is_convert" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_user_id_date_mode_is_convert",
                table: "scores",
                columns: new[] { "user_id", "date", "mode", "is_convert" });
            
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS ix_scores_user_id_stats ON scores(user_id, mode, is_convert) INCLUDE (date, rank, speed_change, beatmap_id)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_accuracy_user_id_mode_is_convert",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_beatmap_id_user_id_mode_is_convert",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_date_mode_is_convert",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_mode_user_id_is_convert",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_pp_user_id_mode_is_convert",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_rank_user_id_mode_is_convert",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_speed_change_user_id_mode_is_convert",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_user_id_date_mode_is_convert",
                table: "scores");
            
            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_user_id_stats",
                table: "scores");

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_accuracy_user_id_mode",
                table: "scores",
                columns: new[] { "accuracy", "user_id", "mode" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_beatmap_id_user_id_mode",
                table: "scores",
                columns: new[] { "beatmap_id", "user_id", "mode" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_date_mode",
                table: "scores",
                columns: new[] { "date", "mode" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_mode_user_id",
                table: "scores",
                columns: new[] { "mode", "user_id" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_pp_user_id_mode",
                table: "scores",
                columns: new[] { "pp", "user_id", "mode" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_rank_user_id_mode",
                table: "scores",
                columns: new[] { "rank", "user_id", "mode" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_speed_change_user_id_mode",
                table: "scores",
                columns: new[] { "speed_change", "user_id", "mode" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_user_id_date_mode",
                table: "scores",
                columns: new[] { "user_id", "date", "mode" });
            
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS ix_scores_user_id_stats ON scores(user_id) INCLUDE (date, rank, speed_change, beatmap_id)");
        }
    }
}
