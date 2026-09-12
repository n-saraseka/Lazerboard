using System;
using System.Collections.Generic;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.OsuEntities.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class ReworkUnlistedScoresTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "scores_pending_deletion");

            migrationBuilder.CreateTable(
                name: "unlisted_scores",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    mode = table.Column<Mode>(type: "mode", nullable: false),
                    beatmap_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    grade = table.Column<Grade>(type: "grade", nullable: false),
                    mod_acronyms = table.Column<List<string>>(type: "text[]", nullable: false),
                    speed_change = table.Column<double>(type: "double precision", nullable: true),
                    accuracy = table.Column<float>(type: "real", nullable: false),
                    combo = table.Column<int>(type: "integer", nullable: false),
                    misses = table.Column<int>(type: "integer", nullable: true),
                    total_score = table.Column<int>(type: "integer", nullable: false),
                    classic_total_score = table.Column<long>(type: "bigint", nullable: false),
                    legacy_total_score = table.Column<int>(type: "integer", nullable: true),
                    pp = table.Column<float>(type: "real", nullable: true),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    score_source = table.Column<ScoreSource>(type: "score_source", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unlisted_scores", x => x.id);
                    table.ForeignKey(
                        name: "fk_unlisted_scores_beatmaps_beatmap_id",
                        column: x => x.beatmap_id,
                        principalTable: "beatmaps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_unlisted_scores_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_unlisted_scores_beatmap_id",
                table: "unlisted_scores",
                column: "beatmap_id");

            migrationBuilder.CreateIndex(
                name: "ix_unlisted_scores_user_id",
                table: "unlisted_scores",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "unlisted_scores");

            migrationBuilder.CreateTable(
                name: "scores_pending_deletion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    score_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    marked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scores_pending_deletion", x => x.id);
                    table.ForeignKey(
                        name: "fk_scores_pending_deletion_scores_score_id",
                        column: x => x.score_id,
                        principalTable: "scores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_scores_pending_deletion_marked_at",
                table: "scores_pending_deletion",
                column: "marked_at");

            migrationBuilder.CreateIndex(
                name: "ix_scores_pending_deletion_score_id",
                table: "scores_pending_deletion",
                column: "score_id",
                unique: true);
        }
    }
}
