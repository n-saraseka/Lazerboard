using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnnecessaryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_accuracy",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_date",
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
                name: "ix_scores_accuracy",
                table: "scores",
                column: "accuracy");

            migrationBuilder.CreateIndex(
                name: "ix_scores_date",
                table: "scores",
                column: "date");

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
