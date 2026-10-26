using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddBetterManiaMillionsIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_total_score_mode",
                table: "scores");
            migrationBuilder.Sql("CREATE INDEX ix_mania_millions ON scores(user_id) WHERE total_score = 1000000 AND mode = 'mania'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX ix_mania_millions");
            migrationBuilder.CreateIndex(
                name: "ix_scores_total_score_mode",
                table: "scores",
                columns: new[] { "total_score", "mode" });
        }
    }
}
