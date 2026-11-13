using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMissesColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "misses",
                table: "unlisted_scores");

            migrationBuilder.DropColumn(
                name: "misses",
                table: "scores");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "misses",
                table: "unlisted_scores",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "misses",
                table: "scores",
                type: "integer",
                nullable: true);
        }
    }
}
