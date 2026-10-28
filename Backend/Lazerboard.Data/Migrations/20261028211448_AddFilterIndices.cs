using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddFilterIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_date_mode",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_mod_acronyms",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_accuracy_mode",
                table: "scores",
                columns: new[] { "accuracy", "mode" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_pp_mode",
                table: "scores",
                columns: new[] { "pp", "mode" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_speed_change_mode",
                table: "scores",
                columns: new[] { "speed_change", "mode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_accuracy_mode",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_pp_mode",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_speed_change_mode",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_date_mode",
                table: "scores",
                columns: new[] { "date", "mode" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_mod_acronyms",
                table: "scores",
                column: "mod_acronyms");
        }
    }
}
