using System;
using Lazerboard.Data.Database.Entities.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddBeatmapsetScanLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:beatmap_status", "approved,graveyard,loved,pending,qualified,ranked,wip")
                .Annotation("Npgsql:Enum:grade", "a,b,c,d,f,s,sh,x,xh")
                .Annotation("Npgsql:Enum:mode", "fruits,mania,osu,taiko")
                .Annotation("Npgsql:Enum:scan_event_type", "rescan_finished,rescan_started,seeding_finished,seeding_started")
                .Annotation("Npgsql:Enum:score_source", "leaderboard_scan,score_fetcher")
                .OldAnnotation("Npgsql:Enum:beatmap_status", "approved,graveyard,loved,pending,qualified,ranked,wip")
                .OldAnnotation("Npgsql:Enum:grade", "a,b,c,d,f,s,sh,x,xh")
                .OldAnnotation("Npgsql:Enum:mode", "fruits,mania,osu,taiko")
                .OldAnnotation("Npgsql:Enum:score_source", "leaderboard_scan,score_fetcher");

            migrationBuilder.CreateTable(
                name: "beatmapset_scan_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    logged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    event_type = table.Column<ScanEventType>(type: "scan_event_type", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beatmapset_scan_logs", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "beatmapset_scan_logs");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:beatmap_status", "approved,graveyard,loved,pending,qualified,ranked,wip")
                .Annotation("Npgsql:Enum:grade", "a,b,c,d,f,s,sh,x,xh")
                .Annotation("Npgsql:Enum:mode", "fruits,mania,osu,taiko")
                .Annotation("Npgsql:Enum:score_source", "leaderboard_scan,score_fetcher")
                .OldAnnotation("Npgsql:Enum:beatmap_status", "approved,graveyard,loved,pending,qualified,ranked,wip")
                .OldAnnotation("Npgsql:Enum:grade", "a,b,c,d,f,s,sh,x,xh")
                .OldAnnotation("Npgsql:Enum:mode", "fruits,mania,osu,taiko")
                .OldAnnotation("Npgsql:Enum:scan_event_type", "rescan_finished,rescan_started,seeding_finished,seeding_started")
                .OldAnnotation("Npgsql:Enum:score_source", "leaderboard_scan,score_fetcher");
        }
    }
}
