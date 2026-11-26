using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class IncludeModeInRankIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_rank_user_id",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_rank_user_id_mode",
                table: "scores",
                columns: new[] { "rank", "user_id", "mode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_rank_user_id_mode",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_rank_user_id",
                table: "scores",
                columns: new[] { "rank", "user_id" });
        }
    }
}
