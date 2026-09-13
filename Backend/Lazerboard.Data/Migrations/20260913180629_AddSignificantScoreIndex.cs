using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddSignificantScoreIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_beatmap_id",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_beatmap_id_mode_total_score_date",
                table: "scores",
                columns: new[] { "beatmap_id", "mode", "total_score", "date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_beatmap_id_mode_total_score_date",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_beatmap_id",
                table: "scores",
                column: "beatmap_id");
        }
    }
}
