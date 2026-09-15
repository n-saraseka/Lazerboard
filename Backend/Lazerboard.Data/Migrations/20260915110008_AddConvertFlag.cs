using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddConvertFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_convert",
                table: "scores",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_scores_is_convert",
                table: "scores",
                column: "is_convert");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scores_is_convert",
                table: "scores");

            migrationBuilder.DropColumn(
                name: "is_convert",
                table: "scores");
        }
    }
}
