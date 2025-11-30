using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSBackupSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateBasicModelForGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    uri = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "moveset",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    seasonindex = table.Column<int>(type: "integer", nullable: false),
                    previoussetid = table.Column<Guid>(type: "uuid", nullable: true),
                    state = table.Column<string>(type: "text", nullable: false),
                    firstseen = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lastseen = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    preretreathash = table.Column<string>(type: "text", nullable: false),
                    fullhash = table.Column<string>(type: "text", nullable: false),
                    gameid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_moveset", x => x.id);
                    table.ForeignKey(
                        name: "fk_moveset_game_gameid",
                        column: x => x.gameid,
                        principalTable: "game",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_moveset_moveset_previoussetid",
                        column: x => x.previoussetid,
                        principalTable: "moveset",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "unitorders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player = table.Column<string>(type: "text", nullable: false),
                    unittype = table.Column<string>(type: "text", nullable: false),
                    unit = table.Column<string>(type: "text", nullable: false),
                    unitcoast = table.Column<string>(type: "text", nullable: true),
                    result = table.Column<string>(type: "text", nullable: false),
                    resultreason = table.Column<string>(type: "text", nullable: false),
                    discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    movesetid = table.Column<Guid>(type: "uuid", nullable: true),
                    from = table.Column<string>(type: "text", nullable: true),
                    to = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unitorders", x => x.id);
                    table.ForeignKey(
                        name: "fk_unitorders_moveset_movesetid",
                        column: x => x.movesetid,
                        principalTable: "moveset",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_moveset_gameid",
                table: "moveset",
                column: "gameid");

            migrationBuilder.CreateIndex(
                name: "ix_moveset_previoussetid",
                table: "moveset",
                column: "previoussetid");

            migrationBuilder.CreateIndex(
                name: "ix_unitorders_movesetid",
                table: "unitorders",
                column: "movesetid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "unitorders");

            migrationBuilder.DropTable(
                name: "moveset");

            migrationBuilder.DropTable(
                name: "game");
        }
    }
}
