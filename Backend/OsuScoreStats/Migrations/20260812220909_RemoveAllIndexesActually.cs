using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Shared.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAllIndexesActually : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_date",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_mod_acronyms",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_mode",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_pp",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_speed_change",
                table: "scores");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_scores_date",
                table: "scores",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_scores_mod_acronyms",
                table: "scores",
                column: "mod_acronyms");

            migrationBuilder.CreateIndex(
                name: "ix_scores_mode",
                table: "scores",
                column: "mode");

            migrationBuilder.CreateIndex(
                name: "ix_scores_pp",
                table: "scores",
                column: "pp");

            migrationBuilder.CreateIndex(
                name: "ix_scores_speed_change",
                table: "scores",
                column: "speed_change");
        }
    }
}
