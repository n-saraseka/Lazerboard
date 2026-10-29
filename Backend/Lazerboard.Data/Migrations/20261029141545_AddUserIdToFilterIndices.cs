using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToFilterIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_mode_accuracy",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_mode_pp",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_mode_speed_change",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_mode_accuracy_user_id",
                table: "scores",
                columns: new[] { "mode", "accuracy", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_mode_pp_user_id",
                table: "scores",
                columns: new[] { "mode", "pp", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_mode_speed_change_user_id",
                table: "scores",
                columns: new[] { "mode", "speed_change", "user_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_mode_accuracy_user_id",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_mode_pp_user_id",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_mode_speed_change_user_id",
                table: "scores");

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
        }
    }
}
