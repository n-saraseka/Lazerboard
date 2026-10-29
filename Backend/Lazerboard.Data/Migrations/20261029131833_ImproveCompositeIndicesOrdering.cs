using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class ImproveCompositeIndicesOrdering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_accuracy_mode",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_is_convert_date_mode",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_pp_mode",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_speed_change_mode",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_is_convert_date_mode",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_rank_mode",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_is_convert_mode_date",
                table: "scores",
                columns: new[] { "is_convert", "mode", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_mode_accuracy",
                table: "scores",
                columns: new[] { "mode", "accuracy" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_mode_pp",
                table: "scores",
                columns: new[] { "mode", "pp" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_mode_speed_change",
                table: "scores",
                columns: new[] { "mode", "speed_change" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_is_convert_mode_date",
                table: "scores",
                columns: new[] { "user_id", "is_convert", "mode", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_mode_rank",
                table: "scores",
                columns: new[] { "user_id", "mode", "rank" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_is_convert_mode_date",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_mode_accuracy",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_mode_pp",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_mode_speed_change",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_is_convert_mode_date",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_mode_rank",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_accuracy_mode",
                table: "scores",
                columns: new[] { "accuracy", "mode" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_is_convert_date_mode",
                table: "scores",
                columns: new[] { "is_convert", "date", "mode" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_pp_mode",
                table: "scores",
                columns: new[] { "pp", "mode" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_speed_change_mode",
                table: "scores",
                columns: new[] { "speed_change", "mode" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_is_convert_date_mode",
                table: "scores",
                columns: new[] { "user_id", "is_convert", "date", "mode" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_rank_mode",
                table: "scores",
                columns: new[] { "user_id", "rank", "mode" });
        }
    }
}
