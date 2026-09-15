using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddMapsetRankedDateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_beatmapsets_ranked_date",
                table: "beatmapsets",
                column: "ranked_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_beatmapsets_ranked_date",
                table: "beatmapsets");
        }
    }
}
