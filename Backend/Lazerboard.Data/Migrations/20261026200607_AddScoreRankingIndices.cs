using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddScoreRankingIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE INDEX ix_ranking_include_converts ON scores(is_convert, mode, rank, user_id) WHERE is_convert = TRUE");
            migrationBuilder.Sql("CREATE INDEX ix_ranking_exclude_converts ON scores(is_convert, mode, rank, user_id) WHERE is_convert = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX ix_ranking_include_converts");
            migrationBuilder.Sql("DROP INDEX ix_ranking_exclude_converts");
        }
    }
}
