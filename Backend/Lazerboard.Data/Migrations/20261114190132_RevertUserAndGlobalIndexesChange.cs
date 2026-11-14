using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class RevertUserAndGlobalIndexesChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_mode_date",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_mode_date",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_is_convert_mode_date",
                table: "scores",
                columns: new[] { "is_convert", "mode", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_is_convert_mode_date",
                table: "scores",
                columns: new[] { "user_id", "is_convert", "mode", "date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_is_convert_mode_date",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_is_convert_mode_date",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_mode_date",
                table: "scores",
                columns: new[] { "mode", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_mode_date",
                table: "scores",
                columns: new[] { "user_id", "mode", "date" });
        }
    }
}
