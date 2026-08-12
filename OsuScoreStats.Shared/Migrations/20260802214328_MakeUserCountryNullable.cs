using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Shared.Migrations
{
    /// <inheritdoc />
    public partial class MakeUserCountryNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_beatmaps_users_user_id",
                table: "beatmaps");

            migrationBuilder.DropForeignKey(
                name: "fk_users_countries_country_code",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_beatmaps_user_id",
                table: "beatmaps");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "beatmaps");

            migrationBuilder.AlterColumn<string>(
                name: "country_code",
                table: "users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "creator",
                table: "beatmapsets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "beatmapsets",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_beatmapsets_user_id",
                table: "beatmapsets",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_beatmapsets_users_user_id",
                table: "beatmapsets",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_users_countries_country_code",
                table: "users",
                column: "country_code",
                principalTable: "countries",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_beatmapsets_users_user_id",
                table: "beatmapsets");

            migrationBuilder.DropForeignKey(
                name: "fk_users_countries_country_code",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_beatmapsets_user_id",
                table: "beatmapsets");

            migrationBuilder.DropColumn(
                name: "creator",
                table: "beatmapsets");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "beatmapsets");

            migrationBuilder.AlterColumn<string>(
                name: "country_code",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "beatmaps",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_beatmaps_user_id",
                table: "beatmaps",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_beatmaps_users_user_id",
                table: "beatmaps",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_users_countries_country_code",
                table: "users",
                column: "country_code",
                principalTable: "countries",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
