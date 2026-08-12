using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Shared.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSomeUnnecesaryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_accuracy",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_classic_total_score",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_combo",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_rank",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_total_score",
                table: "scores");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_scores_accuracy",
                table: "scores",
                column: "accuracy");

            migrationBuilder.CreateIndex(
                name: "ix_scores_classic_total_score",
                table: "scores",
                column: "classic_total_score");

            migrationBuilder.CreateIndex(
                name: "ix_scores_combo",
                table: "scores",
                column: "combo");

            migrationBuilder.CreateIndex(
                name: "ix_scores_rank",
                table: "scores",
                column: "rank");

            migrationBuilder.CreateIndex(
                name: "ix_scores_total_score",
                table: "scores",
                column: "total_score");
        }
    }
}
