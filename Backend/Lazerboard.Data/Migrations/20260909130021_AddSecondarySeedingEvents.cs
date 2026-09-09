using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuScoreStats.Migrations
{
    /// <inheritdoc />
    public partial class AddSecondarySeedingEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TYPE scan_event_type RENAME VALUE 'seeding_finished' TO 'main_seeding_finished'");
            migrationBuilder.Sql("ALTER TYPE scan_event_type RENAME VALUE 'seeding_started' TO 'main_seeding_started'");
            migrationBuilder.Sql("ALTER TYPE scan_event_type ADD VALUE 'secondary_seeding_finished'");
            migrationBuilder.Sql("ALTER TYPE scan_event_type ADD VALUE 'secondary_seeding_started'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TYPE scan_event_type RENAME VALUE 'main_seeding_finished' TO 'seeding_finished'");
            migrationBuilder.Sql("ALTER TYPE scan_event_type RENAME VALUE 'main_seeding_started' TO 'seeding_started'");
        }
    }
}
