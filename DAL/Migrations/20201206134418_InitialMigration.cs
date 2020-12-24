using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameOptions",
                columns: table => new
                {
                    GameOptionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    BoardWidth = table.Column<int>(nullable: false),
                    BoardHeight = table.Column<int>(nullable: false),
                    CanShipsTouchEnum = table.Column<int>(nullable: false),
                    NextMoveAfterHitEnum = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameOptions", x => x.GameOptionId);
                });

            migrationBuilder.CreateTable(
                name: "Ships",
                columns: table => new
                {
                    ShipId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Size = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ships", x => x.ShipId);
                });

            migrationBuilder.CreateTable(
                name: "GameOptionShips",
                columns: table => new
                {
                    GameOptionShipId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<int>(nullable: false),
                    ShipId = table.Column<int>(nullable: false),
                    GameOptionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameOptionShips", x => x.GameOptionShipId);
                    table.ForeignKey(
                        name: "FK_GameOptionShips_GameOptions_GameOptionId",
                        column: x => x.GameOptionId,
                        principalTable: "GameOptions",
                        principalColumn: "GameOptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameOptionShips_Ships_ShipId",
                        column: x => x.ShipId,
                        principalTable: "Ships",
                        principalColumn: "ShipId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    GameId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameOptionId = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 512, nullable: false),
                    GamePhase = table.Column<int>(nullable: false),
                    Player1Id = table.Column<int>(nullable: false),
                    Player2Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.GameId);
                    table.ForeignKey(
                        name: "FK_Games_GameOptions_GameOptionId",
                        column: x => x.GameOptionId,
                        principalTable: "GameOptions",
                        principalColumn: "GameOptionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameSaves",
                columns: table => new
                {
                    GameSaveId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    CreatedByUser = table.Column<bool>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    SerializedGame = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSaves", x => x.GameSaveId);
                    table.ForeignKey(
                        name: "FK_GameSaves_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    PlayerTurn = table.Column<bool>(nullable: false),
                    PlayerTypeEnum = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_Players_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameShips",
                columns: table => new
                {
                    GameShipId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Size = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 32, nullable: false),
                    IsSunken = table.Column<bool>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameShips", x => x.GameShipId);
                    table.ForeignKey(
                        name: "FK_GameShips_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerBoardStates",
                columns: table => new
                {
                    PlayerBoardStateId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    BoardState = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerBoardStates", x => x.PlayerBoardStateId);
                    table.ForeignKey(
                        name: "FK_PlayerBoardStates_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameOptionShips_GameOptionId",
                table: "GameOptionShips",
                column: "GameOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_GameOptionShips_ShipId_GameOptionId",
                table: "GameOptionShips",
                columns: new[] { "ShipId", "GameOptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_GameOptionId",
                table: "Games",
                column: "GameOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_Player1Id",
                table: "Games",
                column: "Player1Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_Player2Id",
                table: "Games",
                column: "Player2Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameSaves_GameId",
                table: "GameSaves",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameShips_PlayerId",
                table: "GameShips",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerBoardStates_PlayerId",
                table: "PlayerBoardStates",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_GameId",
                table: "Players",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Players_Player1Id",
                table: "Games",
                column: "Player1Id",
                principalTable: "Players",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Players_Player2Id",
                table: "Games",
                column: "Player2Id",
                principalTable: "Players",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_GameOptions_GameOptionId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Players_Player1Id",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Players_Player2Id",
                table: "Games");

            migrationBuilder.DropTable(
                name: "GameOptionShips");

            migrationBuilder.DropTable(
                name: "GameSaves");

            migrationBuilder.DropTable(
                name: "GameShips");

            migrationBuilder.DropTable(
                name: "PlayerBoardStates");

            migrationBuilder.DropTable(
                name: "Ships");

            migrationBuilder.DropTable(
                name: "GameOptions");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
