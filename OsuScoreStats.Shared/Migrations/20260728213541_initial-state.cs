using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OsuScoreStats.Shared.OsuApi.Enums;

#nullable disable

namespace OsuScoreStats.Shared.Migrations
{
    /// <inheritdoc />
    public partial class initialstate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:beatmap_status", "approved,graveyard,loved,pending,qualified,ranked,wip")
                .Annotation("Npgsql:Enum:grade", "a,b,c,d,f,s,sh,x,xh")
                .Annotation("Npgsql:Enum:mode", "fruits,mania,osu,taiko");

            migrationBuilder.CreateTable(
                name: "beatmapsets",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    artist = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    preview_url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beatmapsets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_countries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "beatmaps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    beatmapset_id = table.Column<int>(type: "integer", nullable: false),
                    mode = table.Column<Mode>(type: "mode", nullable: false),
                    difficulty_name = table.Column<string>(type: "text", nullable: false),
                    difficulty = table.Column<float>(type: "real", nullable: false),
                    bpm = table.Column<float>(type: "real", nullable: true),
                    approach_rate = table.Column<float>(type: "real", nullable: false),
                    circle_size = table.Column<float>(type: "real", nullable: false),
                    overall_difficulty = table.Column<float>(type: "real", nullable: false),
                    drain_length = table.Column<float>(type: "real", nullable: false),
                    status = table.Column<BeatmapStatus>(type: "beatmap_status", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beatmaps", x => x.id);
                    table.ForeignKey(
                        name: "fk_beatmaps_beatmapsets_beatmapset_id",
                        column: x => x.beatmapset_id,
                        principalTable: "beatmapsets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    country_code = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_countries_country_code",
                        column: x => x.country_code,
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scores",
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
                    total_score = table.Column<long>(type: "bigint", nullable: false),
                    classic_total_score = table.Column<int>(type: "integer", nullable: false),
                    legacy_total_score = table.Column<int>(type: "integer", nullable: true),
                    pp = table.Column<float>(type: "real", nullable: true),
                    rank = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scores", x => x.id);
                    table.ForeignKey(
                        name: "fk_scores_beatmaps_beatmap_id",
                        column: x => x.beatmap_id,
                        principalTable: "beatmaps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_scores_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_beatmaps_beatmapset_id",
                table: "beatmaps",
                column: "beatmapset_id");

            migrationBuilder.CreateIndex(
                name: "ix_scores_beatmap_id",
                table: "scores",
                column: "beatmap_id");

            migrationBuilder.CreateIndex(
                name: "ix_scores_user_id",
                table: "scores",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_country_code",
                table: "users",
                column: "country_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "scores");

            migrationBuilder.DropTable(
                name: "beatmaps");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "beatmapsets");

            migrationBuilder.DropTable(
                name: "countries");
        }
    }
}
