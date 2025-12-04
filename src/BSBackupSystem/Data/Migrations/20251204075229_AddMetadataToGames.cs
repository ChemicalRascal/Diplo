using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSBackupSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMetadataToGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "creation_time",
                table: "games",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "foreign_id",
                table: "games",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "creation_time",
                table: "games");

            migrationBuilder.DropColumn(
                name: "foreign_id",
                table: "games");
        }
    }
}
