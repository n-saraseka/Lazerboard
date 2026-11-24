using Lazerboard.Data.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class OkayWellIllRevertForNow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_accuracy_user_id",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_mode_user_id",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_pp_user_id",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_rank_user_id",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_speed_change_user_id",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_user_id_beatmap_id",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_user_id_date",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_user_id_mode",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_user_id_rank",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_user_id_speed_change",
                table: "scores");

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_mode_accuracy_user_id",
                table: "scores",
                columns: new[] { "mode", "accuracy", "user_id" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_mode_pp_user_id",
                table: "scores",
                columns: new[] { "mode", "pp", "user_id" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_mode_speed_change_user_id",
                table: "scores",
                columns: new[] { "mode", "speed_change", "user_id" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_user_id",
                table: "scores",
                column: "user_id");

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_user_id_beatmap_id_mode",
                table: "scores",
                columns: new[] { "user_id", "beatmap_id", "mode" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_user_id_is_convert_mode_date",
                table: "scores",
                columns: new[] { "user_id", "is_convert", "mode", "date" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_user_id_rank_mode",
                table: "scores",
                columns: new[] { "user_id", "rank", "mode" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_user_id_speed_change_mode",
                table: "scores",
                columns: new[] { "user_id", "speed_change", "mode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_mode_accuracy_user_id",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_mode_pp_user_id",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_mode_speed_change_user_id",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_user_id",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_user_id_beatmap_id_mode",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_user_id_is_convert_mode_date",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_user_id_rank_mode",
                table: "scores");

            migrationBuilder.DropIndexIfExists(
                name: "ix_scores_user_id_speed_change_mode",
                table: "scores");

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_accuracy_user_id",
                table: "scores",
                columns: new[] { "accuracy", "user_id" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_mode_user_id",
                table: "scores",
                columns: new[] { "mode", "user_id" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_pp_user_id",
                table: "scores",
                columns: new[] { "pp", "user_id" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_rank_user_id",
                table: "scores",
                columns: new[] { "rank", "user_id" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_speed_change_user_id",
                table: "scores",
                columns: new[] { "speed_change", "user_id" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_user_id_beatmap_id",
                table: "scores",
                columns: new[] { "user_id", "beatmap_id" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_user_id_date",
                table: "scores",
                columns: new[] { "user_id", "date" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_user_id_mode",
                table: "scores",
                columns: new[] { "user_id", "mode" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_user_id_rank",
                table: "scores",
                columns: new[] { "user_id", "rank" });

            migrationBuilder.CreateIndexIfNotExists(
                name: "ix_scores_user_id_speed_change",
                table: "scores",
                columns: new[] { "user_id", "speed_change" });
        }
    }
}
