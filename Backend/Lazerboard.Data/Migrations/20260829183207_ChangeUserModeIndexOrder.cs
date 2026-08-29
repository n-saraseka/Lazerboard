using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserModeIndexOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_mode_user_id",
                table: "scores");

            migrationBuilder.DropIndex(
                name: "ix_scores_user_id",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id_mode",
                table: "scores",
                columns: new[] { "user_id", "mode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_user_id_mode",
                table: "scores");

            migrationBuilder.CreateIndex(
                name: "ix_scores_mode_user_id",
                table: "scores",
                columns: new[] { "mode", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id",
                table: "scores",
                column: "user_id");
        }
    }
}
