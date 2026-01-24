using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSBackupSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class GamesHaveOwners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "owner_id",
                table: "games",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_games_owner_id",
                table: "games",
                column: "owner_id");

            migrationBuilder.AddForeignKey(
                name: "fk_games_users_owner_id",
                table: "games",
                column: "owner_id",
                principalTable: "system_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_games_users_owner_id",
                table: "games");

            migrationBuilder.DropIndex(
                name: "ix_games_owner_id",
                table: "games");

            migrationBuilder.DropColumn(
                name: "owner_id",
                table: "games");
        }
    }
}
