using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSBackupSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class CleanUpModelForGameReaderOutput : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "to_coast",
                table: "unit_orders",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"ALTER TABLE move_sets ALTER COLUMN pre_retreat_hash TYPE integer USING pre_retreat_hash::integer;");

            migrationBuilder.Sql(@"ALTER TABLE move_sets ALTER COLUMN full_hash TYPE integer USING full_hash::integer;");

            migrationBuilder.AddColumn<string>(
                name: "season_name",
                table: "move_sets",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "to_coast",
                table: "unit_orders");

            migrationBuilder.DropColumn(
                name: "season_name",
                table: "move_sets");

            migrationBuilder.Sql(@"ALTER TABLE move_sets ALTER COLUMN pre_retreat_hash TYPE text USING pre_retreat_hash::text;");

            migrationBuilder.Sql(@"ALTER TABLE move_sets ALTER COLUMN full_hash TYPE text USING full_hash::text;");
        }
    }
}
