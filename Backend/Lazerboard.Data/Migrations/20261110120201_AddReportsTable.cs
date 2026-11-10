using System;
using Lazerboard.Data.Database.Entities.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddReportsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:beatmap_status", "approved,graveyard,loved,pending,qualified,ranked,wip")
                .Annotation("Npgsql:Enum:grade", "a,b,c,d,f,s,sh,x,xh")
                .Annotation("Npgsql:Enum:mode", "fruits,mania,osu,taiko")
                .Annotation("Npgsql:Enum:report_status", "in_progress,is_not_restricted,is_restricted")
                .Annotation("Npgsql:Enum:scan_event_type", "beatmap_scan_finished,beatmap_scan_started,main_seeding_finished,main_seeding_started,rescan_finished,rescan_started,restricted_check_finished,restricted_check_started,scan_finished,scan_started,secondary_seeding_finished,secondary_seeding_started,user_check_finished,user_check_started,user_scan_finished,user_scan_started")
                .Annotation("Npgsql:Enum:score_source", "leaderboard_scan,score_fetcher")
                .OldAnnotation("Npgsql:Enum:beatmap_status", "approved,graveyard,loved,pending,qualified,ranked,wip")
                .OldAnnotation("Npgsql:Enum:grade", "a,b,c,d,f,s,sh,x,xh")
                .OldAnnotation("Npgsql:Enum:mode", "fruits,mania,osu,taiko")
                .OldAnnotation("Npgsql:Enum:scan_event_type", "beatmap_scan_finished,beatmap_scan_started,main_seeding_finished,main_seeding_started,rescan_finished,rescan_started,restricted_check_finished,restricted_check_started,scan_finished,scan_started,secondary_seeding_finished,secondary_seeding_started,user_check_finished,user_check_started,user_scan_finished,user_scan_started")
                .OldAnnotation("Npgsql:Enum:score_source", "leaderboard_scan,score_fetcher");

            migrationBuilder.CreateTable(
                name: "user_reports",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reported_user_id = table.Column<int>(type: "integer", nullable: false),
                    reporting_user_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<ReportStatus>(type: "report_status", nullable: false),
                    reported_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_reports_users_reported_user_id",
                        column: x => x.reported_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_reports_users_reporting_user_id",
                        column: x => x.reporting_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_reports_reported_on",
                table: "user_reports",
                column: "reported_on");

            migrationBuilder.CreateIndex(
                name: "ix_user_reports_reported_user_id",
                table: "user_reports",
                column: "reported_user_id",
                unique: true,
                filter: "status = 'in_progress'");

            migrationBuilder.CreateIndex(
                name: "ix_user_reports_reporting_user_id",
                table: "user_reports",
                column: "reporting_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_reports");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:beatmap_status", "approved,graveyard,loved,pending,qualified,ranked,wip")
                .Annotation("Npgsql:Enum:grade", "a,b,c,d,f,s,sh,x,xh")
                .Annotation("Npgsql:Enum:mode", "fruits,mania,osu,taiko")
                .Annotation("Npgsql:Enum:scan_event_type", "beatmap_scan_finished,beatmap_scan_started,main_seeding_finished,main_seeding_started,rescan_finished,rescan_started,restricted_check_finished,restricted_check_started,scan_finished,scan_started,secondary_seeding_finished,secondary_seeding_started,user_check_finished,user_check_started,user_scan_finished,user_scan_started")
                .Annotation("Npgsql:Enum:score_source", "leaderboard_scan,score_fetcher")
                .OldAnnotation("Npgsql:Enum:beatmap_status", "approved,graveyard,loved,pending,qualified,ranked,wip")
                .OldAnnotation("Npgsql:Enum:grade", "a,b,c,d,f,s,sh,x,xh")
                .OldAnnotation("Npgsql:Enum:mode", "fruits,mania,osu,taiko")
                .OldAnnotation("Npgsql:Enum:report_status", "in_progress,is_not_restricted,is_restricted")
                .OldAnnotation("Npgsql:Enum:scan_event_type", "beatmap_scan_finished,beatmap_scan_started,main_seeding_finished,main_seeding_started,rescan_finished,rescan_started,restricted_check_finished,restricted_check_started,scan_finished,scan_started,secondary_seeding_finished,secondary_seeding_started,user_check_finished,user_check_started,user_scan_finished,user_scan_started")
                .OldAnnotation("Npgsql:Enum:score_source", "leaderboard_scan,score_fetcher");
        }
    }
}
