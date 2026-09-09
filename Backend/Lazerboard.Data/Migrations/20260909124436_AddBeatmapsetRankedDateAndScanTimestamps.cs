using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddBeatmapsetRankedDateAndScanTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "finished_scanning_at",
                table: "beatmapsets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "main_finished_processing_at",
                table: "beatmapsets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "main_started_processing_at",
                table: "beatmapsets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ranked_date",
                table: "beatmapsets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "secondary_finished_processing_at",
                table: "beatmapsets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "secondary_started_processing_at",
                table: "beatmapsets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "started_scanning_at",
                table: "beatmapsets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_beatmapsets_finished_scanning_at",
                table: "beatmapsets",
                column: "finished_scanning_at");

            migrationBuilder.CreateIndex(
                name: "ix_beatmapsets_main_started_processing_at",
                table: "beatmapsets",
                column: "main_started_processing_at");

            migrationBuilder.CreateIndex(
                name: "ix_beatmapsets_secondary_finished_processing_at",
                table: "beatmapsets",
                column: "secondary_finished_processing_at");

            migrationBuilder.CreateIndex(
                name: "ix_beatmapsets_secondary_started_processing_at",
                table: "beatmapsets",
                column: "secondary_started_processing_at");

            migrationBuilder.CreateIndex(
                name: "ix_beatmapsets_started_scanning_at",
                table: "beatmapsets",
                column: "started_scanning_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_beatmapsets_finished_scanning_at",
                table: "beatmapsets");

            migrationBuilder.DropIndex(
                name: "ix_beatmapsets_main_started_processing_at",
                table: "beatmapsets");

            migrationBuilder.DropIndex(
                name: "ix_beatmapsets_secondary_finished_processing_at",
                table: "beatmapsets");

            migrationBuilder.DropIndex(
                name: "ix_beatmapsets_secondary_started_processing_at",
                table: "beatmapsets");

            migrationBuilder.DropIndex(
                name: "ix_beatmapsets_started_scanning_at",
                table: "beatmapsets");

            migrationBuilder.DropColumn(
                name: "finished_scanning_at",
                table: "beatmapsets");

            migrationBuilder.DropColumn(
                name: "main_finished_processing_at",
                table: "beatmapsets");

            migrationBuilder.DropColumn(
                name: "main_started_processing_at",
                table: "beatmapsets");

            migrationBuilder.DropColumn(
                name: "ranked_date",
                table: "beatmapsets");

            migrationBuilder.DropColumn(
                name: "secondary_finished_processing_at",
                table: "beatmapsets");

            migrationBuilder.DropColumn(
                name: "secondary_started_processing_at",
                table: "beatmapsets");

            migrationBuilder.DropColumn(
                name: "started_scanning_at",
                table: "beatmapsets");
        }
    }
}
