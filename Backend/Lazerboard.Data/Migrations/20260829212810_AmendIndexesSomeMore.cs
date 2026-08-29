using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AmendIndexesSomeMore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_mode_date",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_mode_rank",
                table: "scores",
                columns: new[] { "mode", "rank" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_date_mode",
                table: "scores",
                columns: new[] { "user_id", "date", "mode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_mode_rank",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_date_mode",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_mode_date",
                table: "scores",
                columns: new[] { "user_id", "mode", "date" });
        }
    }
}
