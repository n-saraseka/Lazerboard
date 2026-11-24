using Microsoft.EntityFrameworkCore.Migrations;

namespace Lazerboard.Data.Migrations;

public static class MigrationBuilderExtensions
{
    extension(MigrationBuilder migrationBuilder)
    {
        public void DropIndexIfExists(string name, string table)
        {
            migrationBuilder.Sql($"DROP INDEX IF EXISTS {name}");
        }

        public void CreateIndexIfNotExists(string name, string column, string table)
        {
            migrationBuilder.Sql($"CREATE INDEX IF NOT EXISTS {name} ON {table} ({column});");
        }
        
        public void CreateIndexIfNotExists(string name, string[] columns, string table)
        {
            migrationBuilder.Sql($"CREATE INDEX IF NOT EXISTS {name} ON {table} ({string.Join(",", columns)});");
        }
    }
}