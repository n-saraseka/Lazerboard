using System.Collections.Generic;
using Lazerboard.Data.Database.Entities.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddUserGroupsColumn : Migration
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
                .Annotation("Npgsql:Enum:user_group", "administrator,moderator,registered_user,supporter,user")
                .OldAnnotation("Npgsql:Enum:beatmap_status", "approved,graveyard,loved,pending,qualified,ranked,wip")
                .OldAnnotation("Npgsql:Enum:cooldown_reason", "too_many_false_reports,too_many_reports")
                .OldAnnotation("Npgsql:Enum:grade", "a,b,c,d,f,s,sh,x,xh")
                .OldAnnotation("Npgsql:Enum:mode", "fruits,mania,osu,taiko")
                .OldAnnotation("Npgsql:Enum:report_status", "in_progress,is_not_restricted,is_restricted")
                .OldAnnotation("Npgsql:Enum:scan_event_type", "beatmap_scan_finished,beatmap_scan_started,main_seeding_finished,main_seeding_started,reported_user_check_finished,reported_user_check_started,rescan_finished,rescan_started,restricted_check_finished,restricted_check_started,scan_finished,scan_started,secondary_seeding_finished,secondary_seeding_started,user_check_finished,user_check_started,user_scan_finished,user_scan_started")
                .OldAnnotation("Npgsql:Enum:score_source", "leaderboard_scan,score_fetcher");

            migrationBuilder.AddColumn<List<UserGroup>>(
                name: "user_groups",
                table: "users",
                type: "user_group[]",
                nullable: false,
                defaultValue: new UserGroup[] {UserGroup.User});
            
            migrationBuilder.Sql("UPDATE users SET user_groups = ARRAY['registered_user'::user_group]::user_group[] WHERE is_registered = TRUE");
            
            migrationBuilder.DropColumn(
                name: "is_registered",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_registered",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
            
            migrationBuilder.Sql("UPDATE users SET is_registered = TRUE where user_groups @> ARRAY['registered_user'::user_group]::user_group[]");
            
            migrationBuilder.DropColumn(
                name: "user_groups",
                table: "users");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:beatmap_status", "approved,graveyard,loved,pending,qualified,ranked,wip")
                .Annotation("Npgsql:Enum:cooldown_reason", "too_many_false_reports,too_many_reports")
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
                .OldAnnotation("Npgsql:Enum:score_source", "leaderboard_scan,score_fetcher")
                .OldAnnotation("Npgsql:Enum:user_group", "administrator,moderator,registered_user,supporter,user");
        }
    }
}
