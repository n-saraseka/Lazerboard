using System;
using Lazerboard.Data.Database.Entities.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddCooldownsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:beatmap_status", "approved,graveyard,loved,pending,qualified,ranked,wip")
                .Annotation("Npgsql:Enum:cooldown_reason", "too_many_false_reports,too_many_reports")
                .Annotation("Npgsql:Enum:grade", "a,b,c,d,f,s,sh,x,xh")
                .Annotation("Npgsql:Enum:mode", "fruits,mania,osu,taiko")
                .Annotation("Npgsql:Enum:report_status", "in_progress,is_not_restricted,is_restricted")
                .Annotation("Npgsql:Enum:scan_event_type", "beatmap_scan_finished,beatmap_scan_started,main_seeding_finished,main_seeding_started,reported_user_check_finished,reported_user_check_started,rescan_finished,rescan_started,restricted_check_finished,restricted_check_started,scan_finished,scan_started,secondary_seeding_finished,secondary_seeding_started,user_check_finished,user_check_started,user_scan_finished,user_scan_started")
                .Annotation("Npgsql:Enum:score_source", "leaderboard_scan,score_fetcher")
                .OldAnnotation("Npgsql:Enum:beatmap_status", "approved,graveyard,loved,pending,qualified,ranked,wip")
                .OldAnnotation("Npgsql:Enum:grade", "a,b,c,d,f,s,sh,x,xh")
                .OldAnnotation("Npgsql:Enum:mode", "fruits,mania,osu,taiko")
                .OldAnnotation("Npgsql:Enum:report_status", "in_progress,is_not_restricted,is_restricted")
                .OldAnnotation("Npgsql:Enum:scan_event_type", "beatmap_scan_finished,beatmap_scan_started,main_seeding_finished,main_seeding_started,reported_user_check_finished,reported_user_check_started,rescan_finished,rescan_started,restricted_check_finished,restricted_check_started,scan_finished,scan_started,secondary_seeding_finished,secondary_seeding_started,user_check_finished,user_check_started,user_scan_finished,user_scan_started")
                .OldAnnotation("Npgsql:Enum:score_source", "leaderboard_scan,score_fetcher");

            migrationBuilder.CreateTable(
                name: "user_cooldowns",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    cooldown_reason = table.Column<CooldownReason>(type: "cooldown_reason", nullable: false),
                    started_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_cooldowns", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_cooldowns_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_cooldowns_user_id",
                table: "user_cooldowns",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_cooldowns");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:beatmap_status", "approved,graveyard,loved,pending,qualified,ranked,wip")
                .Annotation("Npgsql:Enum:grade", "a,b,c,d,f,s,sh,x,xh")
                .Annotation("Npgsql:Enum:mode", "fruits,mania,osu,taiko")
                .Annotation("Npgsql:Enum:report_status", "in_progress,is_not_restricted,is_restricted")
                .Annotation("Npgsql:Enum:scan_event_type", "beatmap_scan_finished,beatmap_scan_started,main_seeding_finished,main_seeding_started,reported_user_check_finished,reported_user_check_started,rescan_finished,rescan_started,restricted_check_finished,restricted_check_started,scan_finished,scan_started,secondary_seeding_finished,secondary_seeding_started,user_check_finished,user_check_started,user_scan_finished,user_scan_started")
                .Annotation("Npgsql:Enum:score_source", "leaderboard_scan,score_fetcher")
                .OldAnnotation("Npgsql:Enum:beatmap_status", "approved,graveyard,loved,pending,qualified,ranked,wip")
                .OldAnnotation("Npgsql:Enum:cooldown_reason", "too_many_false_reports,too_many_reports")
                .OldAnnotation("Npgsql:Enum:grade", "a,b,c,d,f,s,sh,x,xh")
                .OldAnnotation("Npgsql:Enum:mode", "fruits,mania,osu,taiko")
                .OldAnnotation("Npgsql:Enum:report_status", "in_progress,is_not_restricted,is_restricted")
                .OldAnnotation("Npgsql:Enum:scan_event_type", "beatmap_scan_finished,beatmap_scan_started,main_seeding_finished,main_seeding_started,reported_user_check_finished,reported_user_check_started,rescan_finished,rescan_started,restricted_check_finished,restricted_check_started,scan_finished,scan_started,secondary_seeding_finished,secondary_seeding_started,user_check_finished,user_check_started,user_scan_finished,user_scan_started")
                .OldAnnotation("Npgsql:Enum:score_source", "leaderboard_scan,score_fetcher");
        }
    }
}
