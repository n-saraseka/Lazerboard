using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRankModeIndexOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_mode_rank",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_rank_mode",
                table: "scores",
                columns: new[] { "rank", "mode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_rank_mode",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_mode_rank",
                table: "scores",
                columns: new[] { "mode", "rank" });
        }
    }
}
