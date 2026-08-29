using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddDateModeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_mode",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_date_mode",
                table: "scores",
                columns: new[] { "date", "mode" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_mode_date",
                table: "scores",
                columns: new[] { "user_id", "mode", "date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_date_mode",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_mode_date",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_mode",
                table: "scores",
                columns: new[] { "user_id", "mode" });
        }
    }
}
